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
    public class CustomCodeLocator : CSharpSyntaxWalker
    {
        public List<int> CustomCodeBeginLocations { get; set; }
        public List<int> CustomCodeEndLocations { get; set; }

        public CustomCodeLocator() : base(SyntaxWalkerDepth.Trivia)
        {
            this.CustomCodeBeginLocations = new List<int>();
            this.CustomCodeEndLocations = new List<int>();
        }

        public bool IsLocationWithinCustomCode(int location)
        {
            foreach(var b in CustomCodeBeginLocations)
            {
                var e = this.CustomCodeEndLocations.OrderBy(p => p).Where(p => p >= b).First();
                if (location >= b && location <= e)
                    return true;
            }

            return false;
        }

        public override void VisitTrivia(SyntaxTrivia trivia)
        {
            if (trivia.Kind() == SyntaxKind.SingleLineCommentTrivia)
            {
                var text = trivia.ToString();

                if (text.Contains("////BCC/"))
                    this.CustomCodeBeginLocations.Add(trivia.Span.Start);
                if (text.Contains("////ECC/"))
                    this.CustomCodeEndLocations.Add(trivia.Span.Start);
            }

            base.VisitTrivia(trivia);
        }

        public bool IsNodeWithinCustomCode(SyntaxNode node)
        {
            return this.IsLocationWithinCustomCode(node.SpanStart);
        }
    }
}
