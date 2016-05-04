//------------------------------------------------------------------------------
// <copyright file="GenerateDtoCommand.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using DtoGenerator.Logic.Infrastructure;
using DtoGenerator.Logic.UI;
using DtoGenerator.Vsix.UI;
using EnvDTE;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace DtoGenerator.Vsix
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class GenerateDtoCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("71745907-3366-479c-b4db-44ea3344721b");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateDtoCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private GenerateDtoCommand(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new OleMenuCommand(this.MenuItemCallback, menuCommandID);
                menuItem.BeforeQueryStatus += OnCanShowMenuItem;
                commandService.AddCommand(menuItem);
            }
        }

        private void OnCanShowMenuItem(object sender, EventArgs e)
        {
            // get the menu that fired the event
            var menuCommand = sender as OleMenuCommand;
            if (menuCommand != null)
            {
                // start by assuming that the menu will not be shown
                menuCommand.Visible = false;
                menuCommand.Enabled = false;

                var selectedItem = GetSelectedSolutionExplorerItem();
                if (selectedItem == null)
                    return;

                var ext = Path.GetExtension(selectedItem.Name) ?? string.Empty;

                var isCsharp = new EditorFactory().GetLanguageService(ext) == "{694dd9b6-b865-4c5b-ad85-86356e9c88dc}";

                // if not leave the menu hidden
                if (!isCsharp) return;

                menuCommand.Visible = true;
                menuCommand.Enabled = true;
            }
        }

        private ProjectItem GetSelectedSolutionExplorerItem()
        {
            var ide = (EnvDTE80.DTE2)this.ServiceProvider.GetService(typeof(DTE));

            UIHierarchy solutionExplorer = ide.ToolWindows.SolutionExplorer;
            object[] items = solutionExplorer.SelectedItems as object[];
            if (items.Length != 1)
                return null;

            var hierarchyItem = items[0] as EnvDTE.UIHierarchyItem;
            if (hierarchyItem == null)
                return null;

            var projectItem = ide.Solution.FindProjectItem(hierarchyItem.Name);
            if (projectItem == null)
                return null;

            return projectItem;
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static GenerateDtoCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new GenerateDtoCommand(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private async void MenuItemCallback(object sender, EventArgs e)
        {
            var componentModel = (IComponentModel)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SComponentModel));
            var workspace = componentModel.GetService<VisualStudioWorkspace>();

            var selectedItem = this.GetSelectedSolutionExplorerItem();
            var doc = workspace.CurrentSolution
                .GetDocumentIdsWithFilePath(selectedItem.Document.FullName)
                .Select(p => workspace.CurrentSolution.GetDocument(p))
                .FirstOrDefault();

            var vm = new OptionsViewModel(doc);
            var isConfirmed = new OptionsWindow { DataContext = vm }.ShowModal();

            if(isConfirmed == true)
            {
                var project = doc.Project.Solution.Projects
                    .Where(p => p.Name.Contains(vm.DtoLocation.Split('/').First()))
                    .FirstOrDefault();

                var metadata = vm.EntityModel.ConvertToMetadata();

                var compilation = await project.GetCompilationAsync();
                var existingDtoDocument = compilation.GetDocumentForSymbol(project.Solution, metadata.DtoName);

                SyntaxTree existingSyntaxTree = null;
                if (existingDtoDocument != null)
                    existingSyntaxTree = await existingDtoDocument.GetSyntaxTreeAsync();

                var syntaxTree = DtoBuilder.BuildDto(metadata, dtoNamespace: vm.DtoLocation.Replace('/', '.'), existingDto: existingSyntaxTree);
                var formatted = Formatter.Format(syntaxTree.GetRoot(), workspace);

                if (existingDtoDocument == null)
                {
                    var newDoc = project.AddDocument(doc.Name.Replace(".cs", "DTO.cs"), formatted, folders: vm.DtoLocation.Split('/').Skip(1));
                    workspace.TryApplyChanges(newDoc.Project.Solution);
                }
                else
                {
                    var newDoc = existingDtoDocument.WithSyntaxRoot(syntaxTree.GetRoot());
                    workspace.TryApplyChanges(newDoc.Project.Solution);
                }
            }
        }
    }
}
