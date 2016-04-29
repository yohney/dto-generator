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

            var tree = DtoBuilder.BuildDto(metadata, null, "Test.Namespace.Extra.Long");
            Assert.IsNotNull(tree);

            var x = tree.ToString();
        }
    }
}
