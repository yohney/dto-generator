using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DtoGenerator.Logic.Infrastructure.TreeProcessing;
using DtoGenerator.Logic.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;

namespace DtoGenerator.Logic.Infrastructure
{
    public class DtoBuilder
    {
        public static SyntaxTree BuildDto(EntityMetadata entity, SyntaxTree existingDto = null, string dtoNamespace = null)
        {
            CompilationUnitSyntax root = null;

            if (existingDto == null)
            {
                var rawTree = BuildOutline(dtoNamespace, entity);

                var commentAppender = new EmptyTreeCommentAppender();
                root = commentAppender.Visit(rawTree.GetRoot()) as CompilationUnitSyntax;
            }
            else
            {
                var existingRoot = existingDto.GetRoot();

                var finder = new CustomCodeLocator();
                finder.Visit(existingRoot);

                var rewriter = new GeneratedCodeRemover(finder);
                existingRoot = rewriter.Visit(existingRoot);

                if(rewriter.FirstCustomProperty == null)
                {
                    var commentAppender = new EmptyTreeCommentAppender();
                    root = commentAppender.Visit(existingRoot) as CompilationUnitSyntax;
                }
                else
                {
                    var commentWrapper = new CustomCodeCommentWrapper(rewriter);
                    root = commentWrapper.Visit(existingRoot) as CompilationUnitSyntax;
                }
            }

            var generatedPropertiesAppender = new GeneratedPropertiesAppender(entity);
            root = generatedPropertiesAppender.Visit(root) as CompilationUnitSyntax;

            var usingDirective = root.DescendantNodes(p => !(p is ClassDeclarationSyntax))
                .OfType<UsingDirectiveSyntax>()
                .Where(p => p.Name.ToString() == entity.Namespace)
                .FirstOrDefault();

            if (usingDirective == null)
            {
                root = root.WithUsings(root.Usings.Add(entity.Namespace.ToUsing()));
            }

            return SyntaxFactory.SyntaxTree(root);
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
