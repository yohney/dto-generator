﻿using System;
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
using static DtoGenerator.Logic.UI.PropertySelectorViewModel;

namespace DtoGenerator.Logic.Infrastructure
{
    public class DtoBuilder
    {
        public static SyntaxTree BuildDto(EntityMetadata entity, SyntaxTree existingDto = null, string dtoNamespace = null, string mapperNamespace = null, GeneratorProperties generatorProperties = null)
        {
            if (generatorProperties == null)
            {
                generatorProperties = new GeneratorProperties();
            }

            CompilationUnitSyntax root = null;

            if (existingDto == null)
            {
                var rawTree = BuildOutline(dtoNamespace, entity, generatorProperties.StyleCop);

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

            if (generatorProperties.StyleCop)
            {
                var nameSpaceNode = EntityParser.GetNamespaceNode(root);

                if (generatorProperties.GenerateMapper)
                {
                    if (!generatorProperties.UseBIANetMapperInfra)
                    {
                        nameSpaceNode = nameSpaceNode.AppendUsing(mapperNamespace, entity.Namespace);
                    }
                    else
                    {
                        nameSpaceNode = nameSpaceNode.AppendUsing("BIA.Net.Business.DTO.Infrastructure");
                    }
                    nameSpaceNode = nameSpaceNode.AppendUsing("Model");
                }

                if (generatorProperties.AddDataContract)
                    nameSpaceNode = nameSpaceNode.AppendUsing("System.Runtime.Serialization");

                if (generatorProperties.AddDataAnnotations)
                    nameSpaceNode = nameSpaceNode.AppendUsing("System.ComponentModel.DataAnnotations");

                root = root.ReplaceNode(EntityParser.GetNamespaceNode(root), nameSpaceNode);
            }
            else
            {

                if (generatorProperties.GenerateMapper)
                    root = root.AppendUsing(mapperNamespace, entity.Namespace);

                if (generatorProperties.AddDataContract)
                    root = root.AppendUsing("System.Runtime.Serialization");

                if (generatorProperties.AddDataAnnotations)
                    root = root.AppendUsing("System.ComponentModel.DataAnnotations");
            }

            var generatedPropertiesAppender = new GeneratedPropertiesAppender(entity, generatorProperties);
            root = generatedPropertiesAppender.Visit(root) as CompilationUnitSyntax;

            var newLineRemover = new NewLineRemover();
            root = newLineRemover.Visit(root) as CompilationUnitSyntax;

            if(!generatorProperties.GenerateMapper)
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

        private static SyntaxTree BuildOutline(string dtoNamespace, EntityMetadata entity, bool StyleCop)
        {
            using (var stream = typeof(DtoBuilder).Assembly.GetManifestResourceStream($"DtoGenerator.Logic.Infrastructure.Template.Outline" + (StyleCop? "StyleCop.cs": ".cs")))
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
