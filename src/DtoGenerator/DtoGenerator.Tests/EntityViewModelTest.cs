using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DtoGenerator.Logic.Infrastructure;
using DtoGenerator.Logic.UI;
using DtoGenerator.Tests.CodeSamples;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DtoGenerator.Tests
{
    [TestClass]
    public class EntityViewModelTest
    {
        [TestMethod]
        public async Task EntityViewModel_GetRelatedEntity_FromTestSolution()
        {
            var fileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
            var srcDir = FindSrcDir(fileInfo.Directory);
            var solutionPath = new FileInfo(Path.Combine(srcDir.FullName, "DtoGenerator.TestSolution/DtoGenerator.TestSolution.sln"));

            var msWorkspace = MSBuildWorkspace.Create();
            var solution = msWorkspace.OpenSolutionAsync(solutionPath.FullName).Result;

            var personClassDoc = solution.Projects
                .SelectMany(p => p.Documents)
                .Where(p => p.Name == "Person.cs")
                .FirstOrDefault();

            var vm = new EntityViewModel(personClassDoc);
            var relatedEntity = await vm.GetRelatedEntity("City");

            Assert.IsNotNull(relatedEntity);
            Assert.AreEqual("City", relatedEntity.EntityName);
        }

        private DirectoryInfo FindSrcDir(DirectoryInfo current)
        {
            if (current.Name == "src")
                return current;

            return FindSrcDir(current.Parent);
        } 
    }
}
