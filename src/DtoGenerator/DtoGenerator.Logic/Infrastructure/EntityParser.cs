using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DtoGenerator.Logic.Model;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DtoGenerator.Logic.Infrastructure
{
    public class EntityParser
    {
        public static EntityMetadata FromString(string code)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var root = syntaxTree.GetRoot();

            var classNodes = root
                .DescendantNodes(p => !(p is ClassDeclarationSyntax))
                .OfType<ClassDeclarationSyntax>();

            if(classNodes.Count() != 1)
            {
                throw new ArgumentException("Source code to parse must contain exactly one class declaration!");
            }

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
            result.Properties = properties
                .Select(p => new PropertyMetadata()
                {
                    Type = p.Type.ToString(),
                    Name = p.Identifier.Text,
                    IsSimpleProperty = IsSimpleProperty(p),
                    SyntaxNode = p
                })
                .ToList();

            return result;
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
