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
        public void EntityParser_ParseEntity_SimplePropertiesOnly()
        {
            var code = SampleCodeProvider.EntityOnlySimpleProperties;
            var metadata = EntityParser.FromString(code);

            Assert.AreEqual(4, metadata.Properties.Count);

            Assert.IsTrue(metadata.Properties.Any(p => p.Name == "Id" && p.IsSimpleProperty && p.Type == typeof(Int32).Name));
            Assert.IsTrue(metadata.Properties.Any(p => p.Name == "Name" && p.IsSimpleProperty && p.Type == typeof(string).Name));
            Assert.IsTrue(metadata.Properties.Any(p => p.Name == "Date" && p.IsSimpleProperty && p.Type == typeof(DateTime?).Name));
            Assert.IsTrue(metadata.Properties.Any(p => p.Name == "OtherString" && p.IsSimpleProperty && p.Type == typeof(string).Name));
        }
    }
}
