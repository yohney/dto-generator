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
    public class EmptyTreeCommentAppender : CSharpSyntaxRewriter
    {
        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (!node.Identifier.Text.Contains("Mapper"))
            {
                return node.ReplaceToken(node.OpenBraceToken, SyntaxFactory.Token(SyntaxTriviaList.Empty, SyntaxKind.OpenBraceToken, this.GenerateCommentsTrivia(CommentTriviaType.Property)));
            }

            return base.VisitClassDeclaration(node);
        }

        public override SyntaxNode VisitInitializerExpression(InitializerExpressionSyntax node)
        {
            return node.ReplaceToken(node.OpenBraceToken, SyntaxFactory.Token(SyntaxTriviaList.Empty, SyntaxKind.OpenBraceToken, this.GenerateCommentsTrivia(CommentTriviaType.Selector)));
        }

        public override SyntaxNode VisitBlock(BlockSyntax node)
        {
            if (node.FirstAncestorOrSelf<MethodDeclarationSyntax>() != null && node.FirstAncestorOrSelf<MethodDeclarationSyntax>().Identifier.Text == "MapToModel")
            {
                return node.ReplaceToken(node.OpenBraceToken, SyntaxFactory.Token(SyntaxTriviaList.Empty, SyntaxKind.OpenBraceToken, this.GenerateCommentsTrivia(CommentTriviaType.Mapper)));
            }

            return base.VisitBlock(node);
        }


        private SyntaxTriviaList GenerateCommentsTrivia(CommentTriviaType type)
        {
            switch (type)
            {
                case CommentTriviaType.Mapper:
                    return SyntaxFactory.TriviaList(
                        new[]{
                            SyntaxFactory.Comment("////BCMS/ BEGIN CUSTOM MAPPER SECTION "),
                            SyntaxFactory.Comment("////ECMS/ END CUSTOM MAPPER SECTION")});

                case CommentTriviaType.Property:
                    return SyntaxFactory.TriviaList(
                        new[]{
                            SyntaxFactory.Comment("////BCPS/ BEGIN CUSTOM PROPERTY SECTION "),
                            SyntaxFactory.Comment("////ECPS/ END CUSTOM PROPERTY SECTION")});

                case CommentTriviaType.Selector:
                    return SyntaxFactory.TriviaList(
                        new[]{
                            SyntaxFactory.Comment("////BCSS/ BEGIN CUSTOM SELECTOR SECTION "),
                            SyntaxFactory.Comment("////ECSS/ END CUSTOM SELECTOR SECTION")});

            }

            return SyntaxTriviaList.Empty;
        }

        private enum CommentTriviaType
        {
            Property,
            Selector,
            Mapper
        }
    }
}
