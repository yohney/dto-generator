using System;
using System.Linq;
using DtoGenerator.Logic.Infrastructure;
using DtoGenerator.Tests.CodeSamples;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DtoGenerator.Tests
{
    [TestClass]
    public class DtoBuilderTest
    {
        [TestMethod]
        public void DtoBuilder_SimpleEntity_PropertiesAdded()
        {
            var code = SampleCodeProvider.EntityOnlySimpleProperties;
            var metadata = EntityParser.FromString(code);

            var tree = DtoBuilder.BuildDto(metadata, dtoNamespace: "Test.Namespace.Extra.Long");
            Assert.IsNotNull(tree);

            var codeText = tree.ToString();

            foreach (var prop in metadata.Properties)
                Assert.IsTrue(codeText.Contains(prop.Name));
        }

        [TestMethod]
        public void DtoBuilder_SimpleEntityExistingDto_PropertiesAdded()
        {
            var code = SampleCodeProvider.EntityOnlySimpleProperties;
            var metadata = EntityParser.FromString(code);

            var existingDtoTree = CSharpSyntaxTree.ParseText(SampleCodeProvider.EntityOnlySimplePropertiesDTO);

            var tree = DtoBuilder.BuildDto(metadata, existingDto: existingDtoTree);
            Assert.IsNotNull(tree);

            var codeText = tree.ToString();

            foreach (var prop in metadata.Properties)
                Assert.IsTrue(codeText.Contains(prop.Name));

            var idxOfPreviosProperty = codeText.IndexOf("public int CustomProperty { get; set; }");
            var idxOfNewProperty = codeText.IndexOf("public DateTime? Date { get; set; }");

            Assert.IsTrue(idxOfNewProperty < idxOfPreviosProperty);
        }
    }
}
