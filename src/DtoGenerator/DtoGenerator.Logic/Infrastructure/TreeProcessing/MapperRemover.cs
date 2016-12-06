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
    public class MapperRemover : CSharpSyntaxRewriter
    {
        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (node.Identifier.ToString().Contains("Mapper"))
                return null;

            return base.VisitClassDeclaration(node);
        }
    }
}
