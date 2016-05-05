using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DtoGenerator.Tests
{
    public class IntegrationTestBase
    {
        public FileInfo TestSolution
        {
            get
            {
                var fileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
                var srcDir = FindSrcDir(fileInfo.Directory);
                return new FileInfo(Path.Combine(srcDir.FullName, "DtoGenerator.TestSolution/DtoGenerator.TestSolution.sln"));
            }
        }

        private DirectoryInfo FindSrcDir(DirectoryInfo current)
        {
            if (current.Name == "src")
                return current;

            return FindSrcDir(current.Parent);
        }
    }
}
