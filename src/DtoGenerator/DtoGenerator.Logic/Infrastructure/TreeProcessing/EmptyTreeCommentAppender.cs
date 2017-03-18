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
            var result = node.ReplaceToken(node.OpenBraceToken, SyntaxFactory.Token(SyntaxTriviaList.Empty, SyntaxKind.OpenBraceToken, this.GenerateCommentsTrivia()));

            return base.VisitClassDeclaration(result);
        }

        public override SyntaxNode VisitInitializerExpression(InitializerExpressionSyntax node)
        {
            return node.ReplaceToken(node.OpenBraceToken, SyntaxFactory.Token(SyntaxTriviaList.Empty, SyntaxKind.OpenBraceToken, this.GenerateCommentsTrivia()));
        }

        public override SyntaxNode VisitBlock(BlockSyntax node)
        {
            if (node.FirstAncestorOrSelf<MethodDeclarationSyntax>() != null && node.FirstAncestorOrSelf<MethodDeclarationSyntax>().Identifier.Text == "MapToModel")
            {
                return node.ReplaceToken(node.OpenBraceToken, SyntaxFactory.Token(SyntaxTriviaList.Empty, SyntaxKind.OpenBraceToken, this.GenerateCommentsTrivia()));
            }

            return base.VisitBlock(node);
        }

        private SyntaxTriviaList GenerateCommentsTrivia()
        {
            return SyntaxFactory.TriviaList(
                        new[]{
                            SyntaxExtenders.EndOfLineTrivia,
                            SyntaxFactory.Comment(CustomCodePreserver.CustomCodeCommentBegin),
                            SyntaxExtenders.EndOfLineTrivia,
                            SyntaxExtenders.EndOfLineTrivia,
                            SyntaxFactory.Comment(CustomCodePreserver.CustomCodeCommentEnd),
                            SyntaxExtenders.EndOfLineTrivia,
                            SyntaxExtenders.EndOfLineTrivia});
        }
    }
}
