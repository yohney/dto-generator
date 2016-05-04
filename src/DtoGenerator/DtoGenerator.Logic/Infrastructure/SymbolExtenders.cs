using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Microsoft.CodeAnalysis
{
    public static class SymbolExtenders
    {
        public static Document GetDocumentForSymbol(this Compilation compilation, Solution solution, string name)
        {
            var relatedSymbols = compilation.GetSymbolsWithName(p => p == name, SymbolFilter.Type)
                .ToList();

            if (relatedSymbols.Count != 1)
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
