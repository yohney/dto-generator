using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DtoGenerator.Logic.Model
{
    public class PropertyMetadata
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsSimpleProperty { get; set; }

        public PropertyDeclarationSyntax SyntaxNode { get; internal set; }

        public string RelatedEntityName { get; internal set; }
        public bool IsRelation { get; internal set; }
        public bool IsCollection { get; internal set; }
    }
}
