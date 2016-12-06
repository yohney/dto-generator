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
    public class BaseDtoClassLocator : CSharpSyntaxWalker
    {
        public string BaseDtoName { get; set; }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if(!node.Identifier.ToString().Contains("Mapper") && node?.BaseList?.Types != null)
            {
                this.BaseDtoName = node.BaseList.Types[0].Type.ToString();
            }

            base.VisitClassDeclaration(node);
        }
    }
}
