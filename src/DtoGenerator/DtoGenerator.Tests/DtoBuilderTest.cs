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
            {
                Assert.IsTrue(codeText.Contains($"public {prop.Type} {prop.Name} {{ get; set; }}"));
                Assert.IsTrue(codeText.Contains($"{prop.Name} = p.{prop.Name},"));
                Assert.IsFalse(codeText.Contains($",{prop.Name} = p.{prop.Name}"));

                Assert.IsTrue(codeText.Contains($"model.{prop.Name} = dto.{prop.Name};"));
            }
        }

        [TestMethod]
        public void DtoBuilder_SimpleEntityExistingDto_PropertiesAdded()
        {
            var code = SampleCodeProvider.EntityOnlySimpleProperties;
            var metadata = EntityParser.FromString(code);

            var existingDtoTree = CSharpSyntaxTree.ParseText(SampleCodeProvider.EntityOnlySimplePropertiesDto);

            var tree = DtoBuilder.BuildDto(metadata, existingDto: existingDtoTree);
            Assert.IsNotNull(tree);

            var codeText = tree.ToString();

            foreach (var prop in metadata.Properties)
            {
                Assert.IsTrue(codeText.Contains($"public {prop.Type} {prop.Name} {{ get; set; }}"));
                Assert.IsTrue(codeText.Contains($"{prop.Name} = p.{prop.Name},"));
                Assert.IsFalse(codeText.Contains($",{prop.Name} = p.{prop.Name}"));

                Assert.IsTrue(codeText.Contains($"model.{prop.Name} = dto.{prop.Name};"));
            }

            var customCodeBeginIdx = codeText.IndexOf("////BCC/");
            var customCodeEndIdx = codeText.IndexOf("////ECC/");

            var customPropIdx = codeText.IndexOf("public int CustomProperty { get; set; }");
            var genPropIdx = codeText.IndexOf("public DateTime? Date { get; set; }");

            Assert.AreNotEqual(-1, customPropIdx);
            Assert.AreNotEqual(-1, genPropIdx);
            Assert.AreNotEqual(-1, customCodeBeginIdx);
            Assert.AreNotEqual(-1, customCodeEndIdx);

            Assert.IsTrue(customPropIdx > customCodeBeginIdx && customPropIdx < customCodeEndIdx);
            Assert.IsTrue(genPropIdx > customCodeEndIdx || genPropIdx < customCodeBeginIdx);
        }

        [TestMethod]
        public void DtoBuilder_ComplexEntityDto_Regenerated()
        {
            var code = SampleCodeProvider.ComplexEntity;
            var metadata = EntityParser.FromString(code);
            var otherEntityProp = metadata.Properties.Where(p => p.RelatedEntityName == "OtherEntity").Single();
            otherEntityProp.RelationMetadata = EntityParser.FromString(SampleCodeProvider.OtherEntity);

            var existingDtoTree = CSharpSyntaxTree.ParseText(SampleCodeProvider.ComplexEntityDto);

            var tree = DtoBuilder.BuildDto(metadata, existingDto: existingDtoTree);
            Assert.IsNotNull(tree);

            var codeText = tree.ToString();
        }
    }
}
