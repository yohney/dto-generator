using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DtoGenerator.Logic.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DtoGenerator.Logic.Infrastructure
{
    public class DtoBuilder
    {
        public static SyntaxTree BuildDto(EntityMetadata entity, SyntaxTree existingDto, string dtoNamespace)
        {
            var rawTree = existingDto == null ? BuildOutline(dtoNamespace, entity) : ClearGenerated(existingDto);
            var root = rawTree.GetRoot() as CompilationUnitSyntax;

            var classes = root.DescendantNodesAndSelf(p => !(p is ClassDeclarationSyntax))
                .OfType<ClassDeclarationSyntax>()
                .ToList();

            var dtoClass = classes
                .Where(p => p.Identifier.Text == entity.Name + "DTO")
                .First();

            var newClass = dtoClass;

            foreach (var p in entity.Properties)
                newClass = newClass.AddMembers(p.SyntaxNode);

            root = root.ReplaceNode(dtoClass, newClass);

            return SyntaxFactory.SyntaxTree(root);
        }

        private static SyntaxTree ClearGenerated(SyntaxTree existingDto)
        {
            throw new NotImplementedException();
        }

        private static SyntaxTree BuildOutline(string dtoNamespace, EntityMetadata entity)
        {
            using (var stream = typeof(DtoBuilder).Assembly.GetManifestResourceStream($"DtoGenerator.Logic.Infrastructure.Template.Outline.cs"))
            {
                using (var reader = new StreamReader(stream))
                {
                    var sourceCode = reader.ReadToEnd();
                    sourceCode = sourceCode
                        .Replace("#Entity#", entity.Name)
                        .Replace("#Namespace#", dtoNamespace);

                    return CSharpSyntaxTree.ParseText(sourceCode);
                }
            }
        }
    }
}
