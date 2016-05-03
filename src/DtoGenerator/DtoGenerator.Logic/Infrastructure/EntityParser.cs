using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DtoGenerator.Logic.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DtoGenerator.Logic.Infrastructure
{
    public class EntityParser
    {
        public static EntityMetadata FromString(string code)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            return FromSyntaxTree(syntaxTree);
        }

        public static EntityMetadata FromDocument(Document doc)
        {
            var syntaxTree = doc.GetSyntaxTreeAsync().Result;
            return FromSyntaxTree(syntaxTree);
        }

        private static EntityMetadata FromSyntaxTree(SyntaxTree syntaxTree)
        {
            var root = syntaxTree.GetRoot();

            var classNodes = root
                .DescendantNodes(p => !(p is ClassDeclarationSyntax))
                .OfType<ClassDeclarationSyntax>();

            if (classNodes.Count() != 1)
            {
                throw new ArgumentException("Source code to parse must contain exactly one class declaration!");
            }

            var namespaceNode = root
                .DescendantNodes(p => !(p is NamespaceDeclarationSyntax))
                .OfType<NamespaceDeclarationSyntax>()
                .FirstOrDefault();

            var classNode = classNodes
                .Single();

            var properties = classNode
                .DescendantNodes(p => !(p is PropertyDeclarationSyntax))
                .OfType<PropertyDeclarationSyntax>()
                .Where(p => p.Modifiers.Any(m => m.Kind() == SyntaxKind.PublicKeyword))
                .Where(p => p.AccessorList != null)
                .Where(p => p.AccessorList.Accessors.Any(a => a.Kind() == SyntaxKind.GetAccessorDeclaration))
                .Where(p => p.AccessorList.Accessors.Any(a => a.Kind() == SyntaxKind.SetAccessorDeclaration));

            var result = new EntityMetadata();
            result.Name = classNode.Identifier.Text;
            result.Namespace = namespaceNode.Name.ToString();

            result.Properties = properties
                .Select(p => new PropertyMetadata()
                {
                    Type = p.Type.ToString(),
                    Name = p.Identifier.Text,
                    IsSimpleProperty = IsSimpleProperty(p),
                    SyntaxNode = p,
                    IsCollection = IsCollection(p),
                    IsRelation = IsRelation(p),
                    RelatedEntityName = IsRelation(p) ? GetRelatedEntity(p) : null
                })
                .ToList();

            return result;
        }
        

        private static string GetRelatedEntity(PropertyDeclarationSyntax p)
        {
            if (p.Type is GenericNameSyntax)
                return (p.Type as GenericNameSyntax).TypeArgumentList.Arguments.OfType<IdentifierNameSyntax>().Select(i => i.Identifier.Text).Single();

            if (p.Type is IdentifierNameSyntax)
                return (p.Type as IdentifierNameSyntax).Identifier.Text;

            return null;
        }

        private static bool IsRelation(PropertyDeclarationSyntax p)
        {
            return !IsSimpleProperty(p);
        }

        private static bool IsCollection(PropertyDeclarationSyntax p)
        {
            if (p.Type is GenericNameSyntax)
                return true;

            return false;
        }

        private static bool IsSimpleProperty(PropertyDeclarationSyntax propertyNode)
        {
            var nullableType = propertyNode.Type as NullableTypeSyntax;
            if(nullableType != null)
            {
                return IsSimpleType(nullableType.ElementType);
            }

            return IsSimpleType(propertyNode.Type);
        }

        private static bool IsSimpleType(TypeSyntax type)
        {
            if (type.ToString() == "DateTime")
                return true;

            if (type is PredefinedTypeSyntax)
                return true;

            return false;
        }
    }
}
