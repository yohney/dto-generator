using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DtoGenerator.Logic.Infrastructure;
using DtoGenerator.Logic.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace DtoGenerator.Logic.UI
{
    public class OptionsViewModel : ViewModelBase
    {
        public OptionsViewModel()
        {

        }

        public OptionsViewModel(Document doc)
        {
            this._entityModel = new EntityViewModel(doc);

            this.DtoLocation = doc.Project.Solution.Projects
                .SelectMany(p => p.Documents)
                .Where(p => p.Name.ToLower().Contains("dto"))
                .Select(p => p.Project.Name + "/" + string.Join("/", p.Folders))
                .FirstOrDefault();
        }

        private string _dtoLocation;
        public string DtoLocation
        {
            get
            {
                return this._dtoLocation;
            }
            set
            {
                if (value != this._dtoLocation)
                {
                    this._dtoLocation = value;
                    this.InvokePropertyChanged(nameof(DtoLocation));
                }
            }
        }

        private EntityViewModel _entityModel;
        public EntityViewModel EntityModel
        {
            get
            {
                return this._entityModel;
            }
            set
            {
                if (value != this._entityModel)
                {
                    this._entityModel = value;
                    this.InvokePropertyChanged(nameof(EntityModel));
                }
            }
        }

        public EntityMetadata GetMetadata()
        {
            if (this._entityModel != null)
                return this._entityModel.ConvertToMetadata();

            return null;
        }
    }

    public class EntityViewModel : ViewModelBase
    {
        public EntityViewModel(Document doc)
        {
            this.EntityDocument = doc;
            this.Properties = new ObservableCollection<PropertyViewModel>();

            var metadata = EntityParser.FromDocument(doc);
            this.EntityName = metadata.Name;

            foreach(var p in metadata.Properties)
            {
                var propViewModel = new PropertyViewModel(this);
                propViewModel.CanExpand = p.IsRelation && !p.IsCollection;
                propViewModel.Name = p.Name;
                propViewModel.Type = p.IsCollection ? $"Collection<{p.Type}>" : p.Type;
                propViewModel.CanSelect = true;
                this.Properties.Add(propViewModel);
            }
        }

        public Document EntityDocument { get; set; }
        public string EntityName { get; set; }
        public ObservableCollection<PropertyViewModel> Properties { get; set; }

        public async Task<EntityViewModel> GetRelatedEntity(string entityName)
        {
            var compilation = await this.EntityDocument.Project.GetCompilationAsync();
            var relatedSymbols = compilation.GetSymbolsWithName(p => p == entityName, SymbolFilter.Type)
                .ToList();

            if (relatedSymbols.Count != 1)
                return null;

            var location = relatedSymbols
                .Select(p => p.Locations.FirstOrDefault())
                .FirstOrDefault();

            var docId = this.EntityDocument.Project.Solution
                .GetDocumentIdsWithFilePath(location.SourceTree.FilePath)
                .FirstOrDefault();

            var doc = this.EntityDocument.Project.Solution.GetDocument(docId);
            return new EntityViewModel(doc);
        }

        public EntityMetadata ConvertToMetadata()
        {
            var result = EntityParser.FromDocument(this.EntityDocument);

            var selectedProperties = this.Properties
                .Where(p => p.IsSelected)
                .ToList();

            var relatedPropertiesWithSelection = this.Properties
                .Where(p => p.RelatedEntity != null)
                .Where(p => p.RelatedEntity.HasSelectionInSubtree())
                .ToList();

            var toRemoveFromMetadata = new List<Model.PropertyMetadata>();
            foreach(var x in result.Properties)
            {
                if (!selectedProperties.Concat(relatedPropertiesWithSelection).Any(p => p.Name == x.Name))
                {
                    toRemoveFromMetadata.Add(x);
                }
                else if(x.IsRelation && !x.IsCollection)
                {
                    var related = relatedPropertiesWithSelection
                        .Where(p => p.Name == x.Name)
                        .Select(p => p.RelatedEntity)
                        .Single();

                    x.RelationMetadata = related.ConvertToMetadata();
                }
            }

            result.Properties.RemoveAll(p => toRemoveFromMetadata.Contains(p));

            return result;
        }

        public bool HasSelectionInSubtree()
        {
            return this.Properties.Any(p => p.IsSelected) || 
                this.Properties
                    .Where(p => p.RelatedEntity != null)
                    .Any(p => p.RelatedEntity.HasSelectionInSubtree());
        }
    }

    public class PropertyViewModel : ViewModelBase
    {
        private EntityViewModel _entityViewModel;

        public PropertyViewModel(EntityViewModel entityModel)
        {
            this._entityViewModel = entityModel;

            this.ExpandPropertyCommand = new ExpandPropertyCommandImpl(this);
        }

        public string Type { get; set; }
        public string Name { get; set; }

        public string NameFormatted => $"{Name} ({Type})";

        private bool _isSelected;
        public bool IsSelected
        {
            get
            {
                return this._isSelected;
            }
            set
            {
                if (value != this._isSelected)
                {
                    this._isSelected = value;
                    this.InvokePropertyChanged(nameof(IsSelected));

                    if(this._relatedEntity != null)
                    {
                        foreach (var prop in this._relatedEntity.Properties)
                            prop.IsSelected = value;
                    }
                }
            }
        }

        private bool _canExpand;
        public bool CanExpand
        {
            get
            {
                return this._canExpand;
            }
            set
            {
                if (value != this._canExpand)
                {
                    this._canExpand = value;
                    this.InvokePropertyChanged(nameof(CanExpand));
                }
            }
        }

        private bool _canSelect;
        public bool CanSelect
        {
            get
            {
                return this._canSelect;
            }
            set
            {
                if (value != this._canSelect)
                {
                    this._canSelect = value;
                    this.InvokePropertyChanged(nameof(CanSelect));
                }
            }
        }

        private EntityViewModel _relatedEntity;
        public EntityViewModel RelatedEntity
        {
            get
            {
                return this._relatedEntity;
            }
            set
            {
                if (value != this._relatedEntity)
                {
                    this._relatedEntity = value;
                    this.InvokePropertyChanged(nameof(RelatedEntity));
                }
            }
        }

        public ICommand ExpandPropertyCommand { get; set; }

        internal void LoadRelatedEntity()
        {
            this.RelatedEntity = this._entityViewModel.GetRelatedEntity(this.Type).Result;
        }

        public class ExpandPropertyCommandImpl : ICommand
        {
            private PropertyViewModel _viewModel;

            public ExpandPropertyCommandImpl(PropertyViewModel viewModel)
            {
                this._viewModel = viewModel;
            }

            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter)
            {
                return this._viewModel.CanExpand && this._viewModel.RelatedEntity == null;
            }

            public void Execute(object parameter)
            {
                this._viewModel.LoadRelatedEntity();

                if (CanExecuteChanged != null)
                    CanExecuteChanged.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
