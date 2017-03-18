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
        public static SyntaxTree BuildDto(EntityMetadata entity, SyntaxTree existingDto = null, string dtoNamespace = null, string mapperNamespace = null, bool generateMapper = true, bool addContractAttrs = false, bool addDataAnnotations = false)
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

                var customCodePreserver = new CustomCodePreserver();
                root = customCodePreserver.Visit(existingRoot) as CompilationUnitSyntax;
            }

            if(generateMapper)
                root = root.AppendUsing(mapperNamespace, entity.Namespace);

            if (addContractAttrs)
                root = root.AppendUsing("System.Runtime.Serialization");

            if (addDataAnnotations)
                root = root.AppendUsing("System.ComponentModel.DataAnnotations");

            var generatedPropertiesAppender = new GeneratedPropertiesAppender(entity, addContractAttrs, addDataAnnotations);
            root = generatedPropertiesAppender.Visit(root) as CompilationUnitSyntax;

            var newLineRemover = new NewLineRemover();
            root = newLineRemover.Visit(root) as CompilationUnitSyntax;

            if(!generateMapper)
            {
                var mapperRemover = new MapperRemover();
                root = mapperRemover.Visit(root) as CompilationUnitSyntax;
            }

            return SyntaxFactory.SyntaxTree(root);
        }

        public static SyntaxNode BuildMapper(string mapperNamespace)
        {
            using (var stream = typeof(DtoBuilder).Assembly.GetManifestResourceStream($"DtoGenerator.Logic.Infrastructure.Template.MapperBase.cs"))
            {
                using (var reader = new StreamReader(stream))
                {
                    var sourceCode = reader.ReadToEnd();
                    sourceCode = sourceCode
                        .Replace("#Namespace#", mapperNamespace);

                    return CSharpSyntaxTree.ParseText(sourceCode).GetRoot();
                }
            }
        }

        private static SyntaxTree BuildOutline(string dtoNamespace, EntityMetadata entity)
        {
            using (var stream = typeof(DtoBuilder).Assembly.GetManifestResourceStream($"DtoGenerator.Logic.Infrastructure.Template.Outline.cs"))
            {
                using (var reader = new StreamReader(stream))
                {
                    var baseClassDtoReplacement = ": " + entity.BaseClassDtoName;
                    if (baseClassDtoReplacement == ": ")
                        baseClassDtoReplacement = "";

                    var sourceCode = reader.ReadToEnd();
                    sourceCode = sourceCode
                        .Replace("#Entity#", entity.Name)
                        .Replace("#DTO#", entity.DtoName)
                        .Replace("#DTOAdjusted#", entity.DtoName.Replace("DTO", ""))
                        .Replace("#Namespace#", dtoNamespace)
                        .Replace("#Inheritance#", baseClassDtoReplacement);

                    return CSharpSyntaxTree.ParseText(sourceCode);
                }
            }
        }
    }
}
