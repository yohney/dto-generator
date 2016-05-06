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
        public static async Task<OptionsViewModel> Create(Document doc)
        {
            var instance = new OptionsViewModel();

            instance.EntityModel = await EntityViewModel.CreateRecursive(doc, depth: 3);

            var dtoName = instance.EntityModel.EntityName + "DTO";

            var existingDto = doc.Project.Solution.GetDocumentByName(dtoName);
            if(existingDto != null)
            {
                instance.IsDtoLocationEditable = false;
                instance.DtoLocation = existingDto.GetDocumentRelativeLocation();
            }
            else
            {
                instance.IsDtoLocationEditable = true;
                instance.DtoLocation = doc.Project.Solution.GetMostLikelyDtoLocation();
            }

            return instance;
        }

        private OptionsViewModel()
        {

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

        private bool _isDtoLocationEditable;
        public bool IsDtoLocationEditable
        {
            get
            {
                return this._isDtoLocationEditable;
            }
            set
            {
                if (value != this._isDtoLocationEditable)
                {
                    this._isDtoLocationEditable = value;
                    this.InvokePropertyChanged(nameof(IsDtoLocationEditable));
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
        private EntityMetadata _originalMetadata;

        public string EntityName { get; set; }
        public ObservableCollection<PropertyViewModel> Properties { get; set; }

        public static async Task<EntityViewModel> CreateRecursive(Document doc, int depth = 3, bool autoSelect = true, bool canSelectCollections = true)
        {
            var instance = new EntityViewModel();

            instance.Properties = new ObservableCollection<PropertyViewModel>();

            instance._originalMetadata = EntityParser.FromDocument(doc);
            instance.EntityName = instance._originalMetadata.Name;

            foreach (var p in instance._originalMetadata.Properties)
            {
                var propViewModel = new PropertyViewModel(instance);
                propViewModel.Name = p.Name;
                propViewModel.Type = p.Type;
                propViewModel.CanSelect = true;

                if (p.IsCollection && !canSelectCollections)
                    propViewModel.CanSelect = false;

                propViewModel.IsSelected = autoSelect && p.IsSimpleProperty;

                if (p.IsRelation && !p.IsCollection && depth > 0)
                {
                    var relatedDoc = await doc.GetRelatedEntityDocument(p.RelatedEntityName);
                    if(relatedDoc != null)
                    {
                        propViewModel.RelatedEntity = await CreateRecursive(relatedDoc, depth: depth - 1, autoSelect: false, canSelectCollections: false);
                    }
                    else
                    {
                        p.IsRelation = false;
                        p.IsSimpleProperty = true;
                    }
                }

                instance.Properties.Add(propViewModel);
            }

            return instance;
        }

        private EntityViewModel()
        {

        }

        public EntityMetadata ConvertToMetadata()
        {
            var result = this._originalMetadata.Clone();

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
                        .FirstOrDefault();

                    x.RelationMetadata = related?.ConvertToMetadata();
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
    }
}
