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
    public class GeneratedCodeRemover : CSharpSyntaxRewriter
    {
        private CustomCodeLocator _finder;

        public GeneratedCodeRemover(CustomCodeLocator finder)
        {
            this._finder = finder;
        }

        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var memberAccess = node.Expression as MemberAccessExpressionSyntax;
            if(memberAccess != null && memberAccess.Name.ToString() == "MergeWith")
            {
                return base.Visit(memberAccess.Expression);
            }

            return base.VisitInvocationExpression(node);
        }

        public override SyntaxNode VisitBaseList(BaseListSyntax node)
        {
            if (node != null && !node.ToString().Contains("MapperBase"))
            {
                return null;
            }

            return base.VisitBaseList(node);
        }

        public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia)
        {
            if (trivia.Kind() == SyntaxKind.SingleLineCommentTrivia)
            {
                var text = trivia.ToString();

                if (text.Contains("////BCC/") || text.Contains("////ECC/"))
                    return SyntaxFactory.Whitespace("");
            }

            return base.VisitTrivia(trivia);
        }

        public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            if (node.FirstAncestorOrSelf<ClassDeclarationSyntax>().Identifier.Text.Contains("Mapper"))
            {
                if(!this._finder.IsNodeWithinCustomCode(node))
                {
                    return null;
                }
            }

            return base.VisitFieldDeclaration(node);
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            if (!node.FirstAncestorOrSelf<ClassDeclarationSyntax>().Identifier.Text.Contains("Mapper"))
            {
                // check if node is automatically generated (not wrapped inside custom comments)
                if (!this._finder.IsNodeWithinCustomCode(node))
                {
                    return null;
                }
            }

            return base.VisitPropertyDeclaration(node);
        }

        public override SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            if (node.FirstAncestorOrSelf<MethodDeclarationSyntax>() != null && node.FirstAncestorOrSelf<MethodDeclarationSyntax>().Identifier.Text == "MapToModel")
            {
                // this is mapper expression. Check if automatically generated..
                if (!this._finder.IsNodeWithinCustomCode(node))
                {
                    return null;
                }
            }

            return base.VisitExpressionStatement(node);
        }

        public override SyntaxNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            if(node.FirstAncestorOrSelf<PropertyDeclarationSyntax>() != null && 
                node.FirstAncestorOrSelf<PropertyDeclarationSyntax>().Identifier.Text == "SelectorExpression")
            {
                var customExpressions = node.Initializer.Expressions
                .Where(p => this._finder.IsNodeWithinCustomCode(p))
                .Select(p => p.WithoutTrivia())
                .ToList();

                var nodeTokenList = SyntaxFactory.NodeOrTokenList();

                foreach (var existingExp in customExpressions)
                {
                    nodeTokenList = nodeTokenList.Add(existingExp);
                    nodeTokenList = nodeTokenList.Add(SyntaxFactory.Token(SyntaxKind.CommaToken).AppendNewLine());
                }

                var res = node.WithInitializer(node.Initializer
                    .WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.CloseBraceToken))
                    .WithExpressions(SyntaxFactory.SeparatedList<ExpressionSyntax>(nodeTokenList)));

                return res;
            }

            return node;
        }
    }
}
