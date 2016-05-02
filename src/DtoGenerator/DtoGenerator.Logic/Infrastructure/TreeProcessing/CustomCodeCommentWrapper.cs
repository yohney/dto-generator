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
        

        private GeneratedCodeRemover _remover;

        public CustomCodeCommentWrapper(GeneratedCodeRemover remover)
        {
            this._remover = remover;
        }
        
        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            if (node == null)
                return null;

            var result = this.WrapWithComment(
                node,
                "////BCPS/ BEGIN CUSTOM PROPERTY SECTION ",
                "////ECPS/ END CUSTOM PROPERTY SECTION ",
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
                "////BCMS/ BEGIN CUSTOM MAPPER SECTION ",
                "////ECMS/ END CUSTOM MAPPER SECTION ",
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
                "////BCSS/ BEGIN CUSTOM SELECTOR SECTION ",
                "////ECSS/ END CUSTOM SELECTOR SECTION ",
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
            if (_remover.CustomPropertiesCount == 1)
            {
                if (AreNodesEqual(node, firstOccurance))
                {
                    var leadingTrivia = node.GetLeadingTrivia().AddRange(new[] { SyntaxFactory.Comment(leadingTriviaComment), SyntaxFactory.EndOfLine("\n") });
                    var trailingTrivia = node.GetTrailingTrivia().Add(SyntaxFactory.Comment(trailingTriviaComment));

                    return node
                        .WithLeadingTrivia(leadingTrivia)
                        .WithTrailingTrivia(trailingTrivia);
                }
            }
            else
            {
                if (AreNodesEqual(node, firstOccurance))
                {
                    var leadingTrivia = node.GetLeadingTrivia().Add(SyntaxFactory.Comment(leadingTriviaComment));

                    return node
                        .WithLeadingTrivia(leadingTrivia);
                }
                else if (AreNodesEqual(node, lastOccurance))
                {
                    var trailingTrivia = node.GetTrailingTrivia().Add(SyntaxFactory.Comment(trailingTriviaComment));

                    return node
                        .WithTrailingTrivia(trailingTrivia);
                }
            }

            return null;
        }
    }
}
