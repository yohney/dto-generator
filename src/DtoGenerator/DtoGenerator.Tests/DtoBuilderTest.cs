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
            metadata.DtoName = "EntityOnlySimplePropertiesDTO";

            var tree = DtoBuilder.BuildDto(metadata, dtoNamespace: "Test.Namespace.Extra.Long");
            Assert.IsNotNull(tree);

            var codeText = tree.ToString();

            foreach (var prop in metadata.Properties.ToList())
            {
                Assert.IsTrue(codeText.Contains($"public {prop.Type} {prop.Name} {{ get; set; }}"));

                if (prop != metadata.Properties.Last())
                    Assert.IsTrue(codeText.Contains($"{prop.Name} = p.{prop.Name},"));
                else
                    Assert.IsTrue(codeText.Contains($"{prop.Name} = p.{prop.Name}"));

                Assert.IsFalse(codeText.Contains($",{prop.Name} = p.{prop.Name}"));

                Assert.IsTrue(codeText.Contains($"model.{prop.Name} = dto.{prop.Name};"));
            }

            Assert.IsTrue(codeText.Contains("using DtoGenerator.Tests.CodeSamples;"));
        }

        [TestMethod]
        public void DtoBuilder_SimpleEntityExistingDto_PropertiesAdded()
        {
            var code = SampleCodeProvider.EntityOnlySimpleProperties;
            var metadata = EntityParser.FromString(code);
            metadata.DtoName = "EntityOnlySimplePropertiesDTO";

            var existingDtoTree = CSharpSyntaxTree.ParseText(SampleCodeProvider.EntityOnlySimplePropertiesDto);

            var tree = DtoBuilder.BuildDto(metadata, existingDto: existingDtoTree);
            Assert.IsNotNull(tree);

            var codeText = tree.ToString();

            foreach (var prop in metadata.Properties.ToList())
            {
                Assert.IsTrue(codeText.Contains($"public {prop.Type} {prop.Name} {{ get; set; }}"));
                if(prop != metadata.Properties.Last())
                    Assert.IsTrue(codeText.Contains($"{prop.Name} = p.{prop.Name},"));
                else
                    Assert.IsTrue(codeText.Contains($"{prop.Name} = p.{prop.Name}"));
                
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
            metadata.DtoName = "ComplexEntityDTO";
            var otherEntityProp = metadata.Properties.Where(p => p.RelatedEntityName == "OtherEntity").Single();
            otherEntityProp.RelationMetadata = EntityParser.FromString(SampleCodeProvider.OtherEntity);

            var existingDtoTree = CSharpSyntaxTree.ParseText(SampleCodeProvider.ComplexEntityDto);

            var tree = DtoBuilder.BuildDto(metadata, existingDto: existingDtoTree);
            Assert.IsNotNull(tree);

            var codeText = tree.ToString();

            Assert.IsTrue(codeText.Contains("public string OtherNumber { get; set; }"));
            Assert.IsTrue(codeText.Contains("OtherNumber = p.Other != null ? p.Other.Number : default (string),"));

            Assert.IsTrue(codeText.Contains("public IEnumerable<SomethingDTO> List1 { get; set; }"));
            Assert.IsTrue(codeText.Contains("public IEnumerable<SomethingDTO> Enumerable2 { get; set; }"));
            Assert.IsTrue(codeText.Contains("public IEnumerable<SomethingDTO> Collection2 { get; set; }"));

            Assert.IsTrue(codeText.Contains("List1 = p.List1.AsQueryable().Select(this._somethingMapper.SelectorExpression),"));
            Assert.IsTrue(codeText.Contains("Enumerable2 = p.Enumerable2.AsQueryable().Select(this._somethingMapper.SelectorExpression),"));
            Assert.IsTrue(codeText.Contains("Collection2 = p.Collection2.AsQueryable().Select(this._somethingMapper.SelectorExpression)"));

            Assert.IsFalse(codeText.Contains("////BCC/ BEGIN CUSTOM CODE SECTION ////ECC/ END CUSTOM CODE SECTION"));
            Assert.IsFalse(codeText.Contains("////ECC/ END CUSTOM CODE SECTION private SomethingMapper"));
            Assert.AreEqual(codeText.IndexOf("MapToModel"), codeText.LastIndexOf("MapToModel"));

            Assert.IsTrue(codeText.Contains("// Some custom property leading comment - should not dissapear"));
            Assert.IsTrue(codeText.Contains("// Some custom property 2 leading comment - should not dissapear"));
        }

        [TestMethod]
        public void DtoBuilder_EntityWithBase_BaseCallsGenerated()
        {
            var code = SampleCodeProvider.EntityWithBase;
            var metadata = EntityParser.FromString(code);
            metadata.DtoName = "EntityWithBaseDTO";
            metadata.BaseClassDtoName = "EntityBaseDTO";
            metadata.BaseClassName = "EntityBase";

            var tree = DtoBuilder.BuildDto(metadata, dtoNamespace: "Some.Namespace");
            Assert.IsNotNull(tree);

            var codeText = tree.ToString();
            Assert.IsTrue(codeText.Contains("private EntityBaseMapper _entityBaseMapper = new EntityBaseMapper();"));
            Assert.IsTrue(codeText.Contains("})).MergeWith(this._entityBaseMapper.SelectorExpression);"));

            Assert.IsTrue(codeText.Contains("public class EntityWithBaseDTO : EntityBaseDTO"));
            Assert.IsFalse(codeText.Contains("EntityWithBaseDTO : EntityBaseDTO{"));
            Assert.IsFalse(codeText.Contains("EntityWithBaseDTO : EntityBaseDTO {"));

            Assert.IsTrue(codeText.Contains("this._entityBaseMapper.MapToModel(dto,model);"));
        }

        [TestMethod]
        public void DtoBuilder_EntityWithBase_DataAnnotations()
        {
            var code = SampleCodeProvider.SampleTable1;
            var metadata = EntityParser.FromString(code);
            metadata.DtoName = "SampleTable1DTO";

            var tree = DtoBuilder.BuildDto(metadata, dtoNamespace: "Some.Namespace",addDataAnnotations:true);
            Assert.IsNotNull(tree);

            var codeText = tree.ToString();
            Assert.IsTrue(codeText.Contains("[Required]"));
            Assert.IsTrue(codeText.Contains("[StringLength(10)]"));
            Assert.IsTrue(codeText.Contains("[StringLength(200),Required]"));
            Assert.IsTrue(codeText.Contains("[DisplayFormat(DataFormatString = \"{0:dd/MM/yyyy}\", ApplyFormatInEditMode = true)]"));
            Assert.IsTrue(codeText.Contains("using System.ComponentModel.DataAnnotations;"));
            Assert.IsFalse(codeText.Contains("SuppressMessage"));
        }

        [TestMethod]
        public void DtoBuilder_EntityWithBase_DataAnnotations_MetadataType()
        {
            var code = SampleCodeProvider.SampleTable3;
            var metadata = EntityParser.FromString(code);
            metadata.DtoName = "SampleTable3DTO";

            var tree = DtoBuilder.BuildDto(metadata, dtoNamespace: "Some.Namespace", addDataAnnotations: true);
            Assert.IsNotNull(tree);

            var codeText = tree.ToString();
            Assert.IsTrue(codeText.Contains("[MetadataType(typeof(SampleTable3MD))]"));
        }

        [TestMethod]
        public void DtoBuilder_EntityWithBase_NoDataAnnotations()
        {
            var code = SampleCodeProvider.SampleTable2;
            var metadata = EntityParser.FromString(code);
            metadata.DtoName = "SampleTable2DTO";

            var tree = DtoBuilder.BuildDto(metadata, dtoNamespace: "Some.Namespace", addDataAnnotations: false);
            Assert.IsNotNull(tree);

            var codeText = tree.ToString();
            Assert.IsFalse(codeText.Contains("[Required]"));
            Assert.IsFalse(codeText.Contains("[StringLength(10)]"));
            Assert.IsFalse(codeText.Contains("using System.ComponentModel.DataAnnotations;"));
            Assert.IsFalse(codeText.Contains("[DataContract]"));
            Assert.IsFalse(codeText.Contains("[DataMember]"));
            Assert.IsFalse(codeText.Contains("using System.Runtime.Serialization;"));
        }

        [TestMethod]
        public void DtoBuilder_EntityWithBase_DataAnnotations_And_DataContract()
        {
            var code = SampleCodeProvider.SampleTable2;
            var metadata = EntityParser.FromString(code);
            metadata.DtoName = "SampleTable2DTO";

            var tree = DtoBuilder.BuildDto(metadata, dtoNamespace: "Some.Namespace", addContractAttrs: true, addDataAnnotations: true);
            Assert.IsNotNull(tree);

            var codeText = tree.ToString();
            Assert.IsTrue(codeText.Contains("[Required]"));
            Assert.IsTrue(codeText.Contains("[StringLength(10)]"));
            Assert.IsTrue(codeText.Contains("using System.ComponentModel.DataAnnotations;"));
            Assert.IsTrue(codeText.Contains("[DataContract]"));
            Assert.IsTrue(codeText.Contains("[DataMember]"));
            Assert.IsTrue(codeText.Contains("using System.Runtime.Serialization;"));
        }
    }
}
