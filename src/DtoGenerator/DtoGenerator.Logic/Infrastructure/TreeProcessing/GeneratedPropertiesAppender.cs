using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DtoGenerator.Logic.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DtoGenerator.Logic.Infrastructure.TreeProcessing
{
    public class GeneratedPropertiesAppender : CSharpSyntaxRewriter
    {
        private EntityMetadata _metadata;

        public GeneratedPropertiesAppender(EntityMetadata metadata)
        {
            this._metadata = metadata;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (node.Identifier.Text == this._metadata.Name + "DTO")
            {
                var membersList = node.Members;
                foreach (var prop in this.GenerateProperties(_metadata, ""))
                    membersList = membersList.Add(prop);

                return node.WithMembers(membersList);
            }

            if (node.Identifier.Text.Contains("Mapper"))
            {
                int insertIndex = 0;
                var membersList = node.Members;
                foreach (var prop in _metadata.Properties.Where(p => p.IsCollection))
                {
                    var newField = SyntaxExtenders.DeclareField(type: GenerateMapperTypeName(prop.RelatedEntityName), autoCreateNew: true);
                    membersList = membersList.Insert(insertIndex++, newField);
                }

                return base.VisitClassDeclaration(node.WithMembers(membersList));
            }

            return base.VisitClassDeclaration(node);
        }

        public override SyntaxNode VisitInitializerExpression(InitializerExpressionSyntax node)
        {
            if (node.FirstAncestorOrSelf<PropertyDeclarationSyntax>().Identifier.Text == "SelectorExpression")
            {
                return node.AddExpressions(GenerateInitializerExpressions(_metadata, "", "p.").ToArray());
            }

            return base.VisitInitializerExpression(node);
        }

        public override SyntaxNode VisitBlock(BlockSyntax node)
        {
            if (node.FirstAncestorOrSelf<MethodDeclarationSyntax>() != null && node.FirstAncestorOrSelf<MethodDeclarationSyntax>().Identifier.Text == "MapToModel")
            {
                var statements = node.Statements;
                foreach (var prop in _metadata.Properties.Where(p => !p.IsRelation))
                {
                    var st = SyntaxExtenders.AssignmentStatement("model." + prop.Name, "dto." + prop.Name);
                    statements = statements.Add(st);
                }

                return node.WithStatements(statements);
            }

            return base.VisitBlock(node);
        }

        private IEnumerable<ExpressionSyntax> GenerateInitializerExpressions(EntityMetadata metadata, string propPrefix, string selectorPrefix)
        {
            foreach (var prop in metadata.Properties)
            {
                if (prop.IsCollection)
                {
                    var methodName = (selectorPrefix + prop.Name + ".Select");
                    var selectorProperty = ("this." + GenerateMapperFieldName(prop.RelatedEntityName) + ".SelectorExpression");
                    yield return SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.IdentifierName(prop.Name),
                            methodName.ToMethodInvocation(selectorProperty.ToMemberAccess()))
                        .NormalizeWhitespace(elasticTrivia: true)
                        .AppendNewLine();
                }
                else if (prop.IsRelation && prop.RelationMetadata != null)
                {
                    foreach (var x in GenerateInitializerExpressions(prop.RelationMetadata, propPrefix + prop.RelatedEntityName, selectorPrefix + prop.RelatedEntityName + "."))
                        yield return x;
                }
                else
                {
                    yield return SyntaxExtenders.AssignmentExpression(propPrefix + prop.Name, selectorPrefix + prop.Name).AppendNewLine();
                }
            }
        }

        private IEnumerable<PropertyDeclarationSyntax> GenerateProperties(EntityMetadata metadata, string prefix)
        {
            foreach (var prop in metadata.Properties)
            {
                if (prop.IsRelation && !prop.IsCollection && prop.RelationMetadata != null)
                {
                    foreach (var x in GenerateProperties(prop.RelationMetadata, prefix + prop.RelatedEntityName))
                        yield return x;
                }
                else if(prop.IsCollection || prop.IsSimpleProperty)
                {
                    TypeSyntax type = null;
                    var identifier = prefix + prop.Name;

                    if (prop.IsCollection)
                    {
                        type = prop.RelatedEntityName.ToCollectionType("ICollection");
                    }
                    else
                    {
                        type = SyntaxFactory.IdentifierName(prop.Type);
                    }

                    yield return SyntaxExtenders.DeclareAutoProperty(type, identifier); 
                }
            }
        }
        private string GenerateMapperFieldName(string relatedEntityName)
        {
            var mapperType = relatedEntityName + "Mapper";
            return "_" + char.ToLower(mapperType[0]) + mapperType.Substring(1);
        }

        private string GenerateMapperTypeName(string relatedEntityName)
        {
            return relatedEntityName + "Mapper";
        }

    }
}
