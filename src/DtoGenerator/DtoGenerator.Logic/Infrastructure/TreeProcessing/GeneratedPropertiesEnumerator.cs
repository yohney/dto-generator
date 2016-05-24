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
    public class GeneratedPropertiesEnumerator : CSharpSyntaxWalker
    {
        public List<string> GeneratedProperties { get; set; }

        private CustomCodeLocator _finder;

        public GeneratedPropertiesEnumerator(CustomCodeLocator finder)
        {
            this.GeneratedProperties = new List<string>();

            this._finder = finder;
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            if (!node.FirstAncestorOrSelf<ClassDeclarationSyntax>().Identifier.Text.Contains("Mapper"))
            {
                // check if node is automatically generated (not wrapped inside custom comments)
                if (!this._finder.IsNodeWithinCustomCode(node))
                {
                    this.GeneratedProperties.Add(node.Identifier.Text);
                }
            }
        }
    }
}
