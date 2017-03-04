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
using System.Windows;
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
            try
            {
                var componentModel = (IComponentModel)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SComponentModel));
                var workspace = componentModel.GetService<VisualStudioWorkspace>();
                
                var selectedItem = this.GetSelectedSolutionExplorerItem();
                Microsoft.CodeAnalysis.Document doc = null;

                if(selectedItem != null && selectedItem.Name != null && !selectedItem.Name.EndsWith(".cs"))
                {
                    VsShellUtilities.ShowMessageBox(this.package, "Generate DTO action can only be invoked on CSharp files.", "Error",
                       OLEMSGICON.OLEMSGICON_WARNING,
                       OLEMSGBUTTON.OLEMSGBUTTON_OK,
                       OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

                    return;
                }

                if (selectedItem?.Document != null)
                {
                    doc = workspace.CurrentSolution.GetDocumentByFilePath(selectedItem.Document.FullName);
                }
                else if (selectedItem != null)
                {
                    var file = selectedItem.Name;
                    var projectName = selectedItem.ContainingProject.Name;

                    var docs = workspace.CurrentSolution.Projects
                        .Where(p => p.Name == projectName)
                        .SelectMany(p => p.Documents)
                        .Where(d => d.Name == file)
                        .ToList();

                    if(docs.Count == 0)
                    {
                        VsShellUtilities.ShowMessageBox(this.package, "Shitty exception - cannot get current selected solution item :/// . Try opening desired document, and then activating this command.", "Error",
                            OLEMSGICON.OLEMSGICON_WARNING,
                            OLEMSGBUTTON.OLEMSGBUTTON_OK,
                            OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                    }

                    if(docs.Count > 1)
                    {
                        VsShellUtilities.ShowMessageBox(this.package, "Multiple documents with same name exist - cannot get current selected solution item. Try opening desired document, and then activating this command.", "Error",
                            OLEMSGICON.OLEMSGICON_WARNING,
                            OLEMSGBUTTON.OLEMSGBUTTON_OK,
                            OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                    }

                    doc = docs.FirstOrDefault();
                }
                else
                {
                    VsShellUtilities.ShowMessageBox(this.package, "Shitty exception - cannot get current selected solution item :/// . Try opening desired document, and then activating this command.", "Error",
                        OLEMSGICON.OLEMSGICON_WARNING,
                        OLEMSGBUTTON.OLEMSGBUTTON_OK,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

                    return;
                }
                var possibleProjects = doc.GetPossibleProjects();

                var vmBasic = BasicOptionsViewModel.Create(possibleProjects, doc.Name.Replace(".cs", ""), doc.Project.Solution.GetMostLikelyDtoLocation());
                var shouldProceed = new BasicOptionsWindow { DataContext = vmBasic }.ShowModal();

                if (shouldProceed != true)
                    return;

                var existingDoc = doc.Project.Solution.GetDocumentByLocation(vmBasic.DtoLocation, vmBasic.DtoName);
                if(existingDoc != null)
                {
                    var result = VsShellUtilities.ShowMessageBox(this.package, "There is already a DTO class in the specified location. Press OK if you would like to regenerate it, or cancel to choose different name.", "Warninig - regenerate DTO?",
                        OLEMSGICON.OLEMSGICON_WARNING,
                        OLEMSGBUTTON.OLEMSGBUTTON_OKCANCEL,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

                    if (result != 1)
                        return;
                }

                var vm = await PropertySelectorViewModel.Create(doc, vmBasic.DtoName, vmBasic.DtoLocation, existingDto: existingDoc);
                var isConfirmed = new PropertySelectorWindow() { DataContext = vm }.ShowModal();

                if (isConfirmed == true)
                {
                    var modifiedSolution = await doc.Project.Solution
                        .WriteDto(vm.DtoLocation, vm.EntityModel.ConvertToMetadata(), vm.GenerateMapper, vm.AddDataContract, vm.AddDataAnnotations);

                    var changedDocIds = SolutionParser.GetDocumentIdsToOpen(modifiedSolution, workspace.CurrentSolution);

                    var ok = workspace.TryApplyChanges(modifiedSolution);

                    if (!ok)
                    {
                        VsShellUtilities.ShowMessageBox(this.package, "Unable to generate DTO. Please try again (could not apply changes).", "Error", OLEMSGICON.OLEMSGICON_WARNING, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                    }
                    else
                    {
                        foreach(var docId in changedDocIds)
                        {
                            try
                            {
                                workspace.OpenDocument(docId);
                            }
                            catch(Exception)
                            {
                                // Do nothing, unable to open the document.
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                try
                {
                    VsShellUtilities.ShowMessageBox(this.package, ex.Message, "An exception has occurred. Please c/p stack trace to project website (https://github.com/yohney/dto-generator), with brief description of the problem.",
                        OLEMSGICON.OLEMSGICON_WARNING,
                        OLEMSGBUTTON.OLEMSGBUTTON_OK,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

                    var tmpFile = Path.GetTempFileName();

                    string stackTrace = "";

                    Exception tmp = ex;
                    while (tmp != null)
                    {
                        stackTrace += tmp.StackTrace;
                        stackTrace += "\n----------------------------\n\n";
                        tmp = ex.InnerException;
                    }


                    File.WriteAllText(tmpFile, stackTrace);

                    VsShellUtilities.OpenBrowser("file:///" + tmpFile);

                }
                catch(Exception innerEx)
                {
                    VsShellUtilities.ShowMessageBox(this.package, innerEx.Message, "An exception has occurred. Unable to write stack trace to TEMP directory.",
                        OLEMSGICON.OLEMSGICON_WARNING,
                        OLEMSGBUTTON.OLEMSGBUTTON_OK,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                }
            }
        }
    }
}
