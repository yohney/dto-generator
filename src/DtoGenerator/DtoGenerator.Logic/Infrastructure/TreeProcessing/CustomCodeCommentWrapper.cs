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

            return base.VisitFieldDeclaration(result);
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

            return base.VisitPropertyDeclaration(result);
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

            return result;
        }

        public override SyntaxNode VisitInitializerExpression(InitializerExpressionSyntax node)
        {
            var originalList = node.ChildNodesAndTokens();
            for (int i = 0; i < originalList.Count; i++)
            {
                if (!originalList[i].IsNode)
                    continue;

                if (AreNodesEqual(this._remover.LastCustomSelector, originalList[i].AsNode()))
                {
                    var oldToken = originalList[i + 1].AsToken();
                    var newToken = oldToken.WithTrailingTrivia(oldToken.TrailingTrivia
                        .Add(SyntaxExtenders.EndOfLineTrivia)
                        .Add(SyntaxFactory.Comment(CustomCodeCommentEnd))
                        .Add(SyntaxExtenders.EndOfLineTrivia));

                    return base.VisitInitializerExpression(node.ReplaceToken(oldToken, newToken));
                }
            }

            return base.VisitInitializerExpression(node);
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
                null);

            return base.VisitAssignmentExpression(result);
        }

        private bool AreNodesEqual(SyntaxNode a, SyntaxNode b)
        {
            if (a == null || b == null)
                return false;

            if (a.Kind() != b.Kind())
                return false;

            if (a.ToString() != b.ToString())
                return false;

            return true;
        }

        private TNode WrapWithComment<TNode>(TNode node, string leadingTriviaComment, string trailingTriviaComment, SyntaxNode firstOccurance, SyntaxNode lastOccurance)
            where TNode: SyntaxNode
        {
            if (AreNodesEqual(node, firstOccurance))
            {
                var leadingTrivia = node.GetLeadingTrivia()
                    .Add(SyntaxExtenders.EndOfLineTrivia)
                    .Add(SyntaxFactory.Comment(leadingTriviaComment))
                    .Add(SyntaxExtenders.EndOfLineTrivia);

                node = node
                    .WithLeadingTrivia(leadingTrivia);
            }

            if (AreNodesEqual(node, lastOccurance))
            {
                var trailingTrivia = node.GetTrailingTrivia()
                    .Add(SyntaxExtenders.EndOfLineTrivia)
                    .Add(SyntaxFactory.Comment(trailingTriviaComment))
                    .Add(SyntaxExtenders.EndOfLineTrivia);

                node = node
                    .WithTrailingTrivia(trailingTrivia);
            }

            return node;
        }
    }
}
