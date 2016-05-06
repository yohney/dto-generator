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
        public int CustomPropertiesCount { get; set; }

        public SyntaxNode FirstCustomMapperMember { get; set; }
        public SyntaxNode LastCustomMapperMember { get; set; }

        public SyntaxNode FirstCustomProperty { get; set; }
        public SyntaxNode LastCustomProperty { get; set; }

        public SyntaxNode FirstCustomSelector { get; set; }
        public SyntaxNode LastCustomSelector { get; set; }

        public SyntaxNode FirstCustomMapperStatement { get; set; }
        public SyntaxNode LastCustomMapperStatement { get; set; }

        private CustomCodeLocator _finder;

        public GeneratedCodeRemover(CustomCodeLocator finder)
        {
            this.CustomPropertiesCount = 0;

            this._finder = finder;
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
                else
                {
                    if (this.FirstCustomMapperMember == null)
                        this.FirstCustomMapperMember = node;

                    this.LastCustomMapperMember = node;
                }

                return null;
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
                else
                {
                    this.CustomPropertiesCount++;

                    if (this.FirstCustomProperty == null)
                        this.FirstCustomProperty = node;

                    this.LastCustomProperty = node;
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
                else
                {
                    if (this.FirstCustomMapperStatement == null)
                        this.FirstCustomMapperStatement = node;

                    this.LastCustomMapperStatement = node;
                }
            }

            return base.VisitExpressionStatement(node);
        }

        public override SyntaxNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
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

            this.FirstCustomSelector = res.Initializer.Expressions.FirstOrDefault();
            this.LastCustomSelector = res.Initializer.Expressions.LastOrDefault();

            return res;
        }
    }
}
