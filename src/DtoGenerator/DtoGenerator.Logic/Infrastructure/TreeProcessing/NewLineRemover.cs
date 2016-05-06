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
    public class NewLineRemover : CSharpSyntaxRewriter
    {
        public override SyntaxToken VisitToken(SyntaxToken token)
        {
            if(token.HasLeadingTrivia && token.LeadingTrivia.Count(p => p.Kind() == SyntaxKind.EndOfLineTrivia) > 1)
            {
                var original = token.LeadingTrivia.ToList();
                token = token.WithLeadingTrivia(NormalizeTriviaList(original));
            }

            if (token.HasTrailingTrivia && token.TrailingTrivia.Count(p => p.Kind() == SyntaxKind.EndOfLineTrivia) > 1)
            {
                var original = token.TrailingTrivia.ToList();
                original.Reverse();
                var normalized = NormalizeTriviaList(original).ToList();
                normalized.Reverse();
                token = token.WithTrailingTrivia(SyntaxFactory.TriviaList(normalized));
            }

            return base.VisitToken(token);
        }

        private SyntaxTriviaList NormalizeTriviaList(List<SyntaxTrivia> originalTrivia)
        {
            var result = new List<SyntaxTrivia>(originalTrivia);

            for(int i = 0; i < result.Count; i++)
            {
                bool shouldDelete = false;

                if (result[i].Kind() == SyntaxKind.WhitespaceTrivia)
                {
                    for(int j = i - 1; j >= 0; j--)
                    {
                        if (result[j].Kind() == SyntaxKind.EndOfLineTrivia)
                        {
                            shouldDelete = true;
                        }
                    }
                }

                if (result[i].Kind() == SyntaxKind.EndOfLineTrivia)
                {
                    for (int j = i - 1; j >= 0; j--)
                    {
                        if (result[j].Kind() == SyntaxKind.EndOfLineTrivia)
                        {
                            shouldDelete = true;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                if (shouldDelete)
                {
                    result.RemoveAt(i);
                    i--;
                }
            }

            return SyntaxFactory.TriviaList(result);
        }
    }
}
