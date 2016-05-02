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
                foreach (var prop in _metadata.Properties)
                    membersList = membersList.Add(prop.SyntaxNode);

                return node.WithMembers(membersList);
            }

            return base.VisitClassDeclaration(node);
        }

        public override SyntaxNode VisitInitializerExpression(InitializerExpressionSyntax node)
        {
            if (node.FirstAncestorOrSelf<PropertyDeclarationSyntax>().Identifier.Text == "SelectorExpression")
            {
                var expressions = new List<ExpressionSyntax>();

                foreach (var prop in this._metadata.Properties)
                {
                    var exp = SyntaxFactory.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactory.IdentifierName(prop.Name),
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("p"),
                            SyntaxFactory.IdentifierName(prop.Name)));

                    expressions.Add(exp);
                }

                return node.AddExpressions(expressions.ToArray());
            }

            return base.VisitInitializerExpression(node);
        }

        public override SyntaxNode VisitBlock(BlockSyntax node)
        {
            if (node.FirstAncestorOrSelf<MethodDeclarationSyntax>() != null && node.FirstAncestorOrSelf<MethodDeclarationSyntax>().Identifier.Text == "MapToModel")
            {
                var statements = node.Statements;
                foreach (var prop in _metadata.Properties)
                {
                    var statement = SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName("model"),
                                SyntaxFactory.IdentifierName(prop.Name)),
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName("dto"),
                                SyntaxFactory.IdentifierName(prop.Name))));

                    statements = statements.Add(statement);
                }

                return node.WithStatements(statements);
            }

            return base.VisitBlock(node);
        }
    }
}
