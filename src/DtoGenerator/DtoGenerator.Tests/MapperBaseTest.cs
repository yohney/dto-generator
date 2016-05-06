using System;
using System.Linq;
using System.Linq.Expressions;
using DtoGenerator.Logic.Generated.Verification;
using DtoGenerator.Logic.Infrastructure;
using DtoGenerator.Tests.CodeSamples;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DtoGenerator.Tests
{
    [TestClass]
    public class MapperBaseTest
    {
        [TestMethod]
        public void MapperBase_MergeWith_AllPropertiesMerged()
        {
            var persons = MockDataFactory.GetPersons();
            var personMapper = new PersonMapper();

            var dtos = persons.AsQueryable().Select(personMapper.SelectorExpression);

            Assert.AreEqual(2, dtos.Count());
            foreach(var person in persons)
            {
                var correspondingDto = dtos.Where(p => p.Id == person.Id).FirstOrDefault();
                Assert.IsNotNull(correspondingDto);
                Assert.AreEqual(person.Id, correspondingDto.Id);
                Assert.AreEqual(person.FullName, correspondingDto.FullName);
            }
        }
    }
}
