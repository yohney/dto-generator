using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DtoGenerator.Logic.Infrastructure;
using DtoGenerator.Logic.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace Microsoft.CodeAnalysis
{
    public static class SolutionParser
    {
        public static string GetMostLikelyDtoLocation(this Solution solution)
        {
            return solution.Projects
                .SelectMany(p => p.Documents)
                .Where(p => p.Name.ToLower().Contains("dto"))
                .GroupBy(p => p.Project.Name + "/" + string.Join("/", p.Folders))
                .OrderByDescending(p => p.Count())
                .Select(p => p.Key)
                .FirstOrDefault();
        }

        /// <summary>
        /// Generates all required classes and documents and returns modified solution.
        /// With that modified solution it is required to call Workspace.ApplyChanges()
        /// </summary>
        /// <param name="solution"></param>
        /// <param name="dtoLocation"></param>
        /// <param name="metadata"></param>
        /// <returns>Modified solution containing changes to apply to workspace</returns>
        public static async Task<Solution> WriteDto(this Solution solution, string dtoLocation, EntityMetadata metadata)
        {
            var projectName = dtoLocation.Split('/').First();

            var project = solution.Projects
                    .Where(p => p.Name.Contains(projectName))
                    .FirstOrDefault();

            var compilation = await project.GetCompilationAsync();
            var existingDtoDocument = compilation.GetDocumentForSymbol(project.Solution, metadata.DtoName);

            SyntaxTree existingSyntaxTree = null;
            if (existingDtoDocument != null)
                existingSyntaxTree = await existingDtoDocument.GetSyntaxTreeAsync();

            var dtoNamespace = dtoLocation.Replace('/', '.');
            var mapperNamespace = "unknown";

            var mapperDoc = compilation.GetDocumentForSymbol(project.Solution, "MapperBase");
            if(mapperDoc == null)
            {
                var mapperFolderStructure = dtoLocation.Split('/').Skip(1).Concat(new[] { "Infrastructure" });
                mapperNamespace = dtoNamespace + ".Infrastructure";

                project = project.AddDocument("MapperBase.cs", DtoBuilder.BuildMapper(mapperNamespace), folders: mapperFolderStructure).Project;
            }
            else
            {
                var mapperSyntax = await mapperDoc.GetSyntaxRootAsync();
                mapperNamespace = mapperSyntax.DescendantNodesAndSelf(p => !p.IsKind(CSharp.SyntaxKind.NamespaceDeclaration))
                    .OfType<NamespaceDeclarationSyntax>()
                    .Select(p => p.Name.ToString())
                    .FirstOrDefault();
            }

            var syntaxTree = DtoBuilder.BuildDto(metadata, dtoNamespace: dtoNamespace, existingDto: existingSyntaxTree, mapperNamespace: mapperNamespace);

            if (existingDtoDocument == null)
            {
                var formatted = Formatter.Format(syntaxTree.GetRoot(), solution.Workspace);
                var folderStructure = dtoLocation.Split('/').Skip(1);
                var newDoc = project.AddDocument(metadata.DtoName + ".cs", formatted, folders: folderStructure);
                return newDoc.Project.Solution;
            }
            else
            {
                var root = syntaxTree.GetRoot();
                root = Formatter.Format(root, solution.Workspace);

                var newDoc = existingDtoDocument.WithSyntaxRoot(root);
                return newDoc.Project.Solution;
            }
        }

        public static Document GetDocumentByFilePath(this Solution solution, string fullName)
        {
            return solution.GetDocumentIdsWithFilePath(fullName)
                .Select(p => solution.GetDocument(p))
                .FirstOrDefault();
        }

        public static async Task<Document> GetRelatedEntityDocument(this Document doc, string entityName)
        {
            var compilation = await doc.Project.GetCompilationAsync();
            return compilation.GetDocumentForSymbol(doc.Project.Solution, entityName);
        }

        public static Document GetDocumentForSymbol(this Compilation compilation, Solution solution, string name)
        {
            var relatedSymbols = compilation.GetSymbolsWithName(p => p == name, SymbolFilter.Type)
                .ToList();

            if (relatedSymbols.Count != 1)
                return null;

            var symbol = relatedSymbols.First() as ITypeSymbol;
            if (symbol.TypeKind == TypeKind.Enum)
                return null;

            var location = relatedSymbols
                .Select(p => p.Locations.FirstOrDefault())
                .FirstOrDefault();

            var docId = solution
                .GetDocumentIdsWithFilePath(location.SourceTree.FilePath)
                .FirstOrDefault();

            return solution.GetDocument(docId);
        }
    }
}
