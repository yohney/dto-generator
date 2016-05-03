using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DtoGenerator.Logic.Infrastructure.TreeProcessing
{
    public class CustomCodeCommentWrapper : CSharpSyntaxRewriter
    {
        public const string CustomCodeCommentBegin = "////BCC/ BEGIN CUSTOM CODE SECTION ";
        public const string CustomCodeCommentEnd = "////ECC/ END CUSTOM CODE SECTION ";

        private GeneratedCodeRemover _remover;

        public CustomCodeCommentWrapper(GeneratedCodeRemover remover)
        {
            this._remover = remover;
        }

        public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            if (node == null)
                return null;

            var result = this.WrapWithComment(
                node,
                CustomCodeCommentBegin,
                CustomCodeCommentEnd,
                this._remover.FirstCustomMapperMember,
                this._remover.LastCustomMapperMember);

            return result ?? base.VisitFieldDeclaration(node);
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            if (node == null)
                return null;

            var result = this.WrapWithComment(
                node,
                CustomCodeCommentBegin,
                CustomCodeCommentEnd,
                this._remover.FirstCustomProperty,
                this._remover.LastCustomProperty);

            return result ?? base.VisitPropertyDeclaration(node);
        }

        public override SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            if (node == null)
                return null;

            var result = this.WrapWithComment(
                node,
                CustomCodeCommentBegin,
                CustomCodeCommentEnd,
                this._remover.FirstCustomMapperStatement,
                this._remover.LastCustomMapperStatement);

            return result ?? base.VisitExpressionStatement(node);
        }

        public override SyntaxNode VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            if (node == null)
                return null;

            var result = this.WrapWithComment(
                node,
                CustomCodeCommentBegin,
                CustomCodeCommentEnd,
                this._remover.FirstCustomSelector,
                this._remover.LastCustomSelector);

            return result ?? base.VisitAssignmentExpression(node);
        }

        private bool AreNodesEqual(SyntaxNode a, SyntaxNode b)
        {
            if (a.Kind() != b.Kind())
                return false;

            if (a.ToString() != b.ToString())
                return false;

            return true;
        }

        private SyntaxNode WrapWithComment(SyntaxNode node, string leadingTriviaComment, string trailingTriviaComment, SyntaxNode firstOccurance, SyntaxNode lastOccurance)
        {
            if (firstOccurance == null)
                return null;

            if (_remover.CustomPropertiesCount == 1)
            {
                if (AreNodesEqual(node, firstOccurance))
                {
                    var leadingTrivia = node.GetLeadingTrivia().AddRange(new[] { SyntaxFactory.EndOfLine("\r\n"), SyntaxFactory.Comment(leadingTriviaComment), SyntaxFactory.EndOfLine("\r\n") });
                    var trailingTrivia = node.GetTrailingTrivia().Add(SyntaxFactory.EndOfLine("\r\n")).Add(SyntaxFactory.Comment(trailingTriviaComment)).Add(SyntaxFactory.EndOfLine("\r\n"));

                    return node
                        .WithLeadingTrivia(leadingTrivia)
                        .WithTrailingTrivia(trailingTrivia)
                        .NormalizeWhitespace();
                }
            }
            else
            {
                if (AreNodesEqual(node, firstOccurance))
                {
                    var leadingTrivia = node.GetLeadingTrivia().Add(SyntaxFactory.EndOfLine("\r\n")).Add(SyntaxFactory.Comment(leadingTriviaComment)).Add(SyntaxFactory.EndOfLine("\r\n"));

                    return node
                        .WithLeadingTrivia(leadingTrivia)
                        .NormalizeWhitespace();
                }
                else if (AreNodesEqual(node, lastOccurance))
                {
                    var trailingTrivia = node.GetTrailingTrivia().Add(SyntaxFactory.EndOfLine("\r\n")).Add(SyntaxFactory.Comment(trailingTriviaComment)).Add(SyntaxFactory.EndOfLine("\r\n"));

                    return node
                        .WithTrailingTrivia(trailingTrivia)
                        .NormalizeWhitespace();
                }
            }

            return null;
        }
    }
}
