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

            Assert.AreEqual(4, metadata.Properties.Count);

            Assert.IsTrue(metadata.Properties.All(p => p.SyntaxNode != null));

            Assert.IsTrue(metadata.Properties.Any(p => p.Name == "Id" && p.IsSimpleProperty && p.Type == "int"));
            Assert.IsTrue(metadata.Properties.Any(p => p.Name == "Name" && p.IsSimpleProperty && p.Type == "string"));
            Assert.IsTrue(metadata.Properties.Any(p => p.Name == "Date" && p.IsSimpleProperty && p.Type == "DateTime?"));
            Assert.IsTrue(metadata.Properties.Any(p => p.Name == "OtherString" && p.IsSimpleProperty && p.Type == "string"));
        }
    }
}
