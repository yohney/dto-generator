using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.CodeAnalysis.MSBuild;

namespace DtoGenerator.Standalone.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            
            InitializeComponent();

            var fileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
            var srcDir = FindSrcDir(fileInfo.Directory);
            var solutionPath = new FileInfo(System.IO.Path.Combine(srcDir.FullName, "DtoGenerator.TestSolution/DtoGenerator.TestSolution.sln"));

            var msWorkspace = MSBuildWorkspace.Create();
            var solution = msWorkspace.OpenSolutionAsync(solutionPath.FullName).Result;

            foreach (var proj in solution.Projects)
            {
                var x = proj.GetCompilationAsync().Result;
            }

            var personClassDoc = solution.Projects
                .SelectMany(p => p.Documents)
                .Where(p => p.Name == "Person.cs")
                .FirstOrDefault();

            var vm = new Logic.UI.OptionsViewModel(personClassDoc);

            this.container.DataContext = vm;
        }

        private DirectoryInfo FindSrcDir(DirectoryInfo current)
        {
            if (current.Name == "src")
                return current;

            return FindSrcDir(current.Parent);
        }
    }
}
