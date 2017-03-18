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
        private bool _addDataContractAttrs;
        private bool _addDataAnnotations;

        public GeneratedPropertiesAppender(EntityMetadata metadata, bool addDataContractAttrs, bool addDataAnnotations)
        {
            this._addDataContractAttrs = addDataContractAttrs;
            this._addDataAnnotations = addDataAnnotations;
            this._metadata = metadata;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (node.Identifier.Text == this._metadata.DtoName)
            {
                var membersList = node.Members;
                foreach (var prop in this.GenerateProperties(_metadata, ""))
                    membersList = membersList.Add(prop);

                var result = node.WithMembers(membersList);

                if (this._addDataContractAttrs)
                    result = result.WithAttributeLists(SyntaxExtenders.CreateAttributes("DataContract"));
                else
                    result = result.WithAttributeLists(new SyntaxList<AttributeListSyntax>());

                if (this._addDataAnnotations && this._metadata.AttributesList!= null)
                    result = result.AddAttributeLists(this._metadata.AttributesList.ToArray());

                if (this._metadata.BaseClassDtoName != null)
                {
                    result = result.WithBaseList(this._metadata.BaseClassDtoName.ToBaseClassList());
                }

                return result;
            }

            if (node.Identifier.Text.Contains("Mapper"))
            {
                var selectorExpressionProperty = node.Members.OfType<PropertyDeclarationSyntax>()
                    .Where(p => p.Identifier.ToString() == "SelectorExpression")
                    .FirstOrDefault();

                int insertIndex = selectorExpressionProperty == null ? 0 : node.Members.IndexOf(selectorExpressionProperty);
                var membersList = node.Members;
                foreach (var prop in _metadata.Properties.Where(p => p.IsCollection))
                {
                    var newField = SyntaxExtenders.DeclareField(type: GenerateMapperTypeName(prop.RelatedEntityName), autoCreateNew: true);
                    membersList = membersList.Insert(insertIndex++, newField);
                }

                if (this._metadata.BaseClassDtoName != null)
                {
                    var newField = SyntaxExtenders.DeclareField(type: GenerateMapperTypeName(this._metadata.BaseClassDtoName), autoCreateNew: true);
                    membersList = membersList.Insert(insertIndex++, newField);
                }

                return base.VisitClassDeclaration(node.WithMembers(membersList));
            }

            return base.VisitClassDeclaration(node);
        }

        public override SyntaxNode VisitParenthesizedExpression(ParenthesizedExpressionSyntax node)
        {
            var declaringProperty = node.FirstAncestorOrSelf<PropertyDeclarationSyntax>();
            var parentInvocation = node.FirstAncestorOrSelf<InvocationExpressionSyntax>();

            if (declaringProperty != null &&
                parentInvocation == null &&
                declaringProperty.Identifier.ToString() == "SelectorExpression" &&
                !string.IsNullOrWhiteSpace(this._metadata.BaseClassDtoName))
            {
                var mapperField = this.GenerateMapperFieldName(this._metadata.BaseClassDtoName);
                var methodMemberAccess = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, node, SyntaxFactory.IdentifierName("MergeWith"));
                var invocationExpression = SyntaxFactory.InvocationExpression(methodMemberAccess)
                    .WithArgumentList(SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList<ArgumentSyntax>(
                            new[] {
                                SyntaxFactory.Argument($"this.{mapperField}.SelectorExpression".ToMemberAccess()) })));

                return base.Visit(invocationExpression);
            }

            return base.VisitParenthesizedExpression(node);
        }

        public override SyntaxNode VisitInitializerExpression(InitializerExpressionSyntax node)
        {
            var parentProperty = node.FirstAncestorOrSelf<PropertyDeclarationSyntax>();
            var parentInvocation = node.FirstAncestorOrSelf<InvocationExpressionSyntax>();

            if (parentProperty != null && parentProperty.Identifier.Text == "SelectorExpression" ||
                parentInvocation != null && parentInvocation.ToString().Contains("SelectorExpression"))
            {
                var initializerExpressions = GenerateInitializerExpressions(_metadata, "", "p.").ToList();
                var expressionsWithSeparators = node.AddExpressions(initializerExpressions.ToArray()).Expressions.GetWithSeparators();

                // This section is here only to format code well. Namely, in initializer expression, to add comma token after each expression and newline after comma.
                var list = new List<SyntaxNodeOrToken>();
                var expressionTrailingTrivia = new List<SyntaxTrivia>();
                
                foreach(var item in expressionsWithSeparators.ToList())
                {
                    // This is required if we have a custom code trivia which is attached to expression node, but should be attached after comma-token.
                    if (item.IsToken)
                    {
                        expressionTrailingTrivia.Add(SyntaxExtenders.EndOfLineTrivia);
                        list.Add(item.AsToken().WithTrailingTrivia(expressionTrailingTrivia.ToArray()));
                        expressionTrailingTrivia.Clear();
                    }
                        
                    else
                    {
                        expressionTrailingTrivia = item.GetTrailingTrivia().ToList();
                        list.Add(item.WithTrailingTrivia());
                    }
                }

                if(list.Any() && list.Last().IsNode)
                {
                    var item = list.Last();
                    list.Remove(item);
                    list.Add(item.WithTrailingTrivia(expressionTrailingTrivia));
                }

                return node.WithExpressions(SyntaxFactory.SeparatedList<ExpressionSyntax>(list));
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

                if (!string.IsNullOrWhiteSpace(this._metadata.BaseClassDtoName))
                {
                    var mapperField = this.GenerateMapperFieldName(this._metadata.BaseClassDtoName);
                    var st = SyntaxExtenders.InvocationStatement($"this.{mapperField}.MapToModel", "dto", "model");
                    statements = statements.Add(st);
                }

                return node.WithStatements(statements);
            }

            return base.VisitBlock(node);
        }

        private IEnumerable<ExpressionSyntax> GenerateInitializerExpressions(EntityMetadata metadata, string propPrefix, string selectorPrefix, bool verifyNotNull = false)
        {
            foreach (var prop in metadata.Properties)
            {
                if (prop.IsCollection)
                {
                    var queryableMethod = (selectorPrefix + prop.Name + ".AsQueryable").ToMethodInvocation();

                    var selectMethodMember = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        queryableMethod,
                        "Select".ToName());

                    var selectorProperty = ("this." + GenerateMapperFieldName(prop.RelatedEntityName) + ".SelectorExpression");
                    yield return SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.IdentifierName(prop.Name).AppendWhitespace(),
                            selectMethodMember.ToMethodInvocation(selectorProperty.ToMemberAccess()).PrependWhitespace());
                }
                else if (prop.IsRelation && prop.RelationMetadata != null)
                {
                    foreach (var x in GenerateInitializerExpressions(prop.RelationMetadata, propPrefix + prop.Name, selectorPrefix + prop.Name + ".", verifyNotNull: true))
                        yield return x;
                }
                else
                {
                    yield return SyntaxExtenders.AssignmentExpression(propPrefix + prop.Name, selectorPrefix + prop.Name, prop.Type, verifyRightNotNull: verifyNotNull);
                }
            }
        }

        private IEnumerable<PropertyDeclarationSyntax> GenerateProperties(EntityMetadata metadata, string prefix)
        {
            foreach (var prop in metadata.Properties)
            {
                if (prop.IsRelation && !prop.IsCollection && prop.RelationMetadata != null)
                {
                    foreach (var x in GenerateProperties(prop.RelationMetadata, prefix + prop.Name))
                        yield return x;
                }
                else if (prop.IsCollection || prop.IsSimpleProperty)
                {
                    TypeSyntax type = null;
                    var identifier = prefix + prop.Name;

                    if (prop.IsCollection)
                    {
                        type = (prop.RelatedEntityName + "DTO").ToCollectionType("IEnumerable");
                    }
                    else
                    {
                        type = SyntaxFactory.IdentifierName(prop.Type);
                    }

                    var result = SyntaxExtenders.DeclareAutoProperty(type, identifier);

                    

                    if (this._addDataContractAttrs)
                        result = result.WithAttributeLists(SyntaxExtenders.CreateAttributes("DataMember"));
                    else
                        result = result.WithAttributeLists(new SyntaxList<AttributeListSyntax>());

                    if (this._addDataAnnotations && prop.AttributesList != null)
                        result = result.AddAttributeLists(prop.AttributesList.ToArray());

                    yield return result;
                }
            }
        }
        private string GenerateMapperFieldName(string relatedEntityName)
        {
            if (string.IsNullOrWhiteSpace(relatedEntityName))
                return null;

            relatedEntityName = relatedEntityName.Replace("DTO", "");

            var mapperType = relatedEntityName + "Mapper";
            return "_" + char.ToLower(mapperType[0]) + mapperType.Substring(1);
        }

        private string GenerateMapperTypeName(string relatedEntityName)
        {
            relatedEntityName = relatedEntityName.Replace("DTO", "");

            return relatedEntityName + "Mapper";
        }

    }
}
