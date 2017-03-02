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
    public class EntityParserTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EntityParser_ParseEntity_NoClassDeclarations()
        {
            var code = SampleCodeProvider.NoClass;
            var metadata = EntityParser.FromString(code);

            Assert.Fail("Should not reach here.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EntityParser_ParseEntity_MultipleClassDeclarations()
        {
            var code = SampleCodeProvider.MultipleClasses;
            var metadata = EntityParser.FromString(code);

            Assert.Fail("Should not reach here.");
        }

        [TestMethod]
        public void EntityParser_ParseEntity_SimplePropertiesOnly()
        {
            var code = SampleCodeProvider.EntityOnlySimpleProperties;
            var metadata = EntityParser.FromString(code);

            Assert.AreEqual(5, metadata.Properties.Count);

            Assert.IsTrue(metadata.Properties.Any(p => p.Name == "Id" && p.IsSimpleProperty && p.Type == "int"));
            Assert.IsTrue(metadata.Properties.Any(p => p.Name == "Name" && p.IsSimpleProperty && p.Type == "string"));
            Assert.IsTrue(metadata.Properties.Any(p => p.Name == "Date" && p.IsSimpleProperty && p.Type == "DateTime?"));
            Assert.IsTrue(metadata.Properties.Any(p => p.Name == "Date2" && p.IsSimpleProperty && p.Type == "Nullable<System.DateTime>" && !p.IsCollection));
            Assert.IsTrue(metadata.Properties.Any(p => p.Name == "OtherString" && p.IsSimpleProperty && p.Type == "string"));
        }

        [TestMethod]
        public void EntityParser_ParseEntity_WithCollectionProperties()
        {
            var code = SampleCodeProvider.EntityWithCollectionProperties;
            var metadata = EntityParser.FromString(code);

            Assert.AreEqual(6, metadata.Properties.Count);

            Assert.IsTrue(metadata.Properties.All(p => !p.IsSimpleProperty));
            Assert.IsTrue(metadata.Properties.All(p => p.IsCollection));
            Assert.IsTrue(metadata.Properties.All(p => p.IsRelation));
            Assert.IsTrue(metadata.Properties.All(p => p.RelatedEntityName == "Something"));
        }

        [TestMethod]
        public void EntityParser_ParseEntity_ComplexEntity()
        {
            var code = SampleCodeProvider.ComplexEntity;
            var metadata = EntityParser.FromString(code);

            Assert.AreEqual("DtoGenerator.Tests.CodeSamples", metadata.Namespace);
            Assert.AreEqual("ComplexEntity", metadata.Name);
            Assert.AreEqual(11, metadata.Properties.Count);

            Assert.AreEqual(7, metadata.Properties.Count(p => p.IsSimpleProperty));
            Assert.AreEqual(3, metadata.Properties.Count(p => p.IsCollection));
            Assert.AreEqual(1, metadata.Properties.Count(p => p.RelatedEntityName == "OtherEntity"));

            var relatedEntity = metadata.Properties
                .Where(p => p.RelatedEntityName == "OtherEntity")
                .FirstOrDefault();

            Assert.IsTrue(relatedEntity.IsRelation);
            Assert.IsFalse(relatedEntity.IsCollection);
            Assert.IsFalse(relatedEntity.IsSimpleProperty);
        }

        [TestMethod]
        public void EntityParser_ParseEntity_NestedEntity()
        {
            var code = SampleCodeProvider.NestedEntity;
            var metadata = EntityParser.FromString(code);

            Assert.AreEqual("DtoGenerator.Tests.CodeSamples", metadata.Namespace);
            Assert.AreEqual("NestedEntity", metadata.Name);
            Assert.AreEqual(2, metadata.Properties.Count);

            Assert.AreEqual(1, metadata.Properties.Count(p => p.IsSimpleProperty));
            Assert.AreEqual(1, metadata.Properties.Count(p => p.RelatedEntityName == "Nested"));

            var relatedEntity = metadata.Properties
                .Where(p => p.RelatedEntityName == "Nested")
                .FirstOrDefault();

            Assert.IsTrue(relatedEntity.IsRelation);
            Assert.IsFalse(relatedEntity.IsCollection);
            Assert.IsFalse(relatedEntity.IsSimpleProperty);
        }
    }
}
