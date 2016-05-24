using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DtoGenerator.Logic.Infrastructure;
using DtoGenerator.Logic.Infrastructure.TreeProcessing;
using DtoGenerator.Logic.UI;
using DtoGenerator.Tests.CodeSamples;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DtoGenerator.Tests
{
    [TestClass]
    public class EntityViewModelTest : IntegrationTestBase
    {
        [TestMethod]
        public async Task EntityViewModel_GetRelatedEntity_FromTestSolution()
        {
            var msWorkspace = MSBuildWorkspace.Create();
            var solution = msWorkspace.OpenSolutionAsync(this.TestSolution.FullName).Result;

            var personClassDoc = solution.Projects
                .SelectMany(p => p.Documents)
                .Where(p => p.Name == "Person.cs")
                .FirstOrDefault();

            var vm = await EntityViewModel.CreateRecursive(personClassDoc);
            var relatedEntity = vm.Properties
                .Where(p => p.Name == "City")
                .Select(p => p.RelatedEntity)
                .Single();

            Assert.IsNotNull(relatedEntity);
            Assert.AreEqual("City", relatedEntity.EntityName);
            Assert.IsTrue(relatedEntity.Properties.Any(p => p.Name == "UniqueId"));

            msWorkspace.Dispose();
        }

        [TestMethod]
        public async Task WriteDto_ExistingSolution_CityDTORewrite()
        {
            var msWorkspace = MSBuildWorkspace.Create();
            var solution = msWorkspace.OpenSolutionAsync(this.TestSolution.FullName).Result;
            solution = solution.GetIsolatedSolution();

            var personClassDoc = solution.Projects
                .SelectMany(p => p.Documents)
                .Where(p => p.Name == "City.cs")
                .FirstOrDefault();

            var vm = await EntityViewModel.CreateRecursive(personClassDoc);
            vm.DtoName = "CityDTO";
            var dtoLocation = solution.GetMostLikelyDtoLocation();

            var modifiedSolution = await solution.WriteDto(dtoLocation, vm.ConvertToMetadata());
            Assert.IsNotNull(modifiedSolution);

            var cityDto = modifiedSolution.GetChanges(solution)
                .GetProjectChanges().Single()
                .GetChangedDocuments()
                .Select(p => modifiedSolution.GetProject(p.ProjectId).GetDocument(p))
                .Where(p => p.Name == "CityDTO.cs")
                .FirstOrDefault();

            Assert.IsNotNull(cityDto);

            var source = await cityDto.GetTextAsync();
            var sourceCode = source.ToString();

            Assert.AreEqual(3, Regex.Matches(sourceCode, CustomCodeCommentWrapper.CustomCodeCommentBegin).Count);
            Assert.AreEqual(3, Regex.Matches(sourceCode, CustomCodeCommentWrapper.CustomCodeCommentEnd).Count);

            Assert.IsTrue(sourceCode.Contains("CustomProperty = p.Name + p.PostalCode,"));
        }

        [TestMethod]
        public async Task WriteDto_ExistingSolution_Scenario()
        {
            var msWorkspace = MSBuildWorkspace.Create();
            var solution = msWorkspace.OpenSolutionAsync(this.TestSolution.FullName).Result;
            solution = solution.GetIsolatedSolution();

            var personClassDoc = solution.Projects
                .SelectMany(p => p.Documents)
                .Where(p => p.Name == "Person.cs")
                .FirstOrDefault();

            var vm = await EntityViewModel.CreateRecursive(personClassDoc);
            vm.DtoName = "PersonDTO";
            var dtoLocation = solution.GetMostLikelyDtoLocation();

            var modifiedSolution = await solution.WriteDto(dtoLocation, vm.ConvertToMetadata());
            Assert.IsNotNull(modifiedSolution);

            var changeSet = modifiedSolution.GetChanges(solution);
            Assert.AreEqual(1, changeSet.GetProjectChanges().Count());

            var projectChanges = changeSet.GetProjectChanges().Single();
            Assert.AreEqual(2, projectChanges.GetAddedDocuments().Count());

            var addedDocs = projectChanges.GetAddedDocuments()
                .Select(p => modifiedSolution.GetProject(p.ProjectId).GetDocument(p))
                .ToList();

            Assert.IsTrue(addedDocs.Any(p => p.Name == "PersonDTO.cs"));
            Assert.IsTrue(addedDocs.Any(p => p.Name == "MapperBase.cs"));
        }

        [TestMethod]
        public async Task WriteDto_ExistingSolution_PersonContact()
        {
            var msWorkspace = MSBuildWorkspace.Create();
            var solution = msWorkspace.OpenSolutionAsync(this.TestSolution.FullName).Result;
            solution = solution.GetIsolatedSolution();

            var personContactClass = solution.Projects
                .SelectMany(p => p.Documents)
                .Where(p => p.Name == "PersonContact.cs")
                .FirstOrDefault();

            var vm = await EntityViewModel.CreateRecursive(personContactClass);
            vm.DtoName = "PersonContactDTO";

            var typeProp = vm.Properties.Where(p => p.Name == "Type").Single();
            typeProp.IsSelected = true;

            var personProp = vm.Properties.Where(p => p.Name == "Person").Single();
            personProp.IsSelected = true;
            
            foreach(var p in personProp.RelatedEntity.Properties)
            {
                if (p.Name == "FullName" || p.Name == "Contacts" || p.Name == "City")
                    p.IsSelected = true;

                if(p.Name == "City")
                {
                    var cityNameProp = p.RelatedEntity.Properties.Where(c => c.Name == "Name").Single();
                    cityNameProp.IsSelected = true;
                }
            }

            var dtoLocation = solution.GetMostLikelyDtoLocation();

            var modifiedSolution = await solution.WriteDto(dtoLocation, vm.ConvertToMetadata());
            Assert.IsNotNull(modifiedSolution);

            var changeSet = modifiedSolution.GetChanges(solution);
            Assert.AreEqual(1, changeSet.GetProjectChanges().Count());

            var projectChanges = changeSet.GetProjectChanges().Single();
            Assert.AreEqual(2, projectChanges.GetAddedDocuments().Count());

            var addedDocs = projectChanges.GetAddedDocuments()
                .Select(p => modifiedSolution.GetProject(p.ProjectId).GetDocument(p))
                .ToList();

            var personContactDto = addedDocs
                .Where(p => p.Name == "PersonContactDTO.cs")
                .SingleOrDefault();

            Assert.IsNotNull(personContactDto);

            var source = await personContactDto.GetTextAsync();
            var sourceCode = source.ToString();

            Assert.IsTrue(sourceCode.Contains("public ContactType Type { get; set; }"));
        }

        [TestMethod]
        public async Task WriteDto_ExistingSolution_CityDTORewrite2()
        {
            var msWorkspace = MSBuildWorkspace.Create();
            var solution = msWorkspace.OpenSolutionAsync(this.TestSolution.FullName).Result;
            solution = solution.GetIsolatedSolution();

            var cityDoc = solution.Projects
                .SelectMany(p => p.Documents)
                .Where(p => p.Name == "City.cs")
                .FirstOrDefault();

            var existingDto = solution.Projects
                .SelectMany(p => p.Documents)
                .Where(p => p.Name == "CityDTO.cs")
                .FirstOrDefault();

            var dtoLocation = solution.GetMostLikelyDtoLocation();
            var vm = await PropertySelectorViewModel.Create(cityDoc, "CityDTO", dtoLocation, existingDto);

            var countryProp = vm.EntityModel.Properties.Where(p => p.Name == "Country").FirstOrDefault();
            Assert.IsNotNull(countryProp);

            Assert.IsTrue(countryProp.RelatedEntity.Properties.Where(p => p.Name == "Code").Any(p => p.IsSelected));
            Assert.IsFalse(countryProp.RelatedEntity.Properties.Where(p => p.Name == "Name").Any(p => p.IsSelected));

            var modifiedSolution = await solution.WriteDto(dtoLocation, vm.GetMetadata());
            Assert.IsNotNull(modifiedSolution);

            var cityDto = modifiedSolution.GetChanges(solution)
                .GetProjectChanges().Single()
                .GetChangedDocuments()
                .Select(p => modifiedSolution.GetProject(p.ProjectId).GetDocument(p))
                .Where(p => p.Name == "CityDTO.cs")
                .FirstOrDefault();

            Assert.IsNotNull(cityDto);

            var source = await cityDto.GetTextAsync();
            var sourceCode = source.ToString();

            Assert.IsTrue(sourceCode.Contains("public string CountryCode { get; set; }"));
        }

        [TestMethod]
        public async Task WriteDto_ExistingSolution_ScenarioCustomDtoName()
        {
            var msWorkspace = MSBuildWorkspace.Create();
            var solution = msWorkspace.OpenSolutionAsync(this.TestSolution.FullName).Result;
            solution = solution.GetIsolatedSolution();

            var personClassDoc = solution.Projects
                .SelectMany(p => p.Documents)
                .Where(p => p.Name == "Person.cs")
                .FirstOrDefault();

            var dtoLocation = solution.GetMostLikelyDtoLocation();
            var vm = await PropertySelectorViewModel.Create(personClassDoc, "PersonCustomDTO", dtoLocation);

            var modifiedSolution = await solution.WriteDto(dtoLocation, vm.GetMetadata());
            Assert.IsNotNull(modifiedSolution);

            var changeSet = modifiedSolution.GetChanges(solution);
            Assert.AreEqual(1, changeSet.GetProjectChanges().Count());

            var projectChanges = changeSet.GetProjectChanges().Single();
            Assert.AreEqual(2, projectChanges.GetAddedDocuments().Count());

            var addedDocs = projectChanges.GetAddedDocuments()
                .Select(p => modifiedSolution.GetProject(p.ProjectId).GetDocument(p))
                .ToList();

            Assert.IsTrue(addedDocs.Any(p => p.Name == "PersonCustomDTO.cs"));
            Assert.IsTrue(addedDocs.Any(p => p.Name == "MapperBase.cs"));

            var personDtoSource = addedDocs.Where(p => p.Name == "PersonCustomDTO.cs").Single().GetTextAsync().Result.ToString();

            Assert.IsFalse(personDtoSource.Contains("PersonDTO"));
        }
    }
}
