using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DtoGenerator.Logic.Infrastructure.TreeProcessing
{
    public class CustomCodeLocator : CSharpSyntaxWalker
    {
        public int CustomPropertyBegin { get; private set; }
        public int CustomPropertyEnd { get; private set; }

        public int CustomSelectorBegin { get; private set; }
        public int CustomSelectorEnd { get; private set; }

        public int CustomMapperBegin { get; private set; }
        public int CustomMapperEnd { get; private set; }

        public CustomCodeLocator() : base(SyntaxWalkerDepth.Trivia)
        {
        }

        public override void VisitTrivia(SyntaxTrivia trivia)
        {
            if (trivia.Kind() == SyntaxKind.SingleLineCommentTrivia)
            {
                var text = trivia.ToString();

                if (text.Contains("////BCPS/"))
                    this.CustomPropertyBegin = trivia.Span.Start;
                if (text.Contains("////ECPS/"))
                    this.CustomPropertyEnd = trivia.Span.Start;

                if (text.Contains("////BCSS/"))
                    this.CustomSelectorBegin = trivia.Span.Start;
                if (text.Contains("////ECSS/"))
                    this.CustomSelectorEnd = trivia.Span.Start;

                if (text.Contains("////BCMS/"))
                    this.CustomMapperBegin = trivia.Span.Start;
                if (text.Contains("////ECMS/"))
                    this.CustomMapperEnd = trivia.Span.Start;
            }

            base.VisitTrivia(trivia);
        }
    }
}
