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
    public class BasicOptionsViewModel : ViewModelBase
    {
        public static BasicOptionsViewModel Create(List<string> possibleProjects, string entityName, SolutionLocation likelyDtoLocation)
        {
            var instance = new BasicOptionsViewModel();

            instance.EntityName = entityName;
            instance.PossibleProjects = possibleProjects;

            instance.DtoName = entityName + "DTO";
            instance.DtoLocation = likelyDtoLocation ?? new SolutionLocation();

            instance.RecommendedNames = new List<string>();
            instance.RecommendedNames.Add(instance.DtoName);
            instance.RecommendedNames.Add(entityName + "ListDTO");
            instance.RecommendedNames.Add(entityName + "EditDTO");
            instance.RecommendedNames.Add(entityName + "SimpleDTO");
            instance.RecommendedNames.Add(entityName + "ExtendedDTO");

            return instance;
        }

        private BasicOptionsViewModel()
        {

        }

        public string EntityName { get; set; }

        private string _dtoName;
        public string DtoName
        {
            get
            {
                return this._dtoName;
            }
            set
            {
                if (value != this._dtoName)
                {
                    this._dtoName = value;
                    this.InvokePropertyChanged(nameof(DtoName));
                }
            }
        }

        private SolutionLocation _dtoLocation;
        public SolutionLocation DtoLocation
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

        private List<string> _recommendedNames;
        public List<string> RecommendedNames
        {
            get
            {
                return this._recommendedNames;
            }
            set
            {
                if (value != this._recommendedNames)
                {
                    this._recommendedNames = value;
                    this.InvokePropertyChanged(nameof(RecommendedNames));
                }
            }
        }

        private List<string> _possibleProjects;
        public List<string> PossibleProjects
        {
            get
            {
                return this._possibleProjects;
            }
            set
            {
                if (value != this._possibleProjects)
                {
                    this._possibleProjects = value;
                    this.InvokePropertyChanged(nameof(PossibleProjects));
                }
            }
        }

    }

    public class SolutionLocation : ViewModelBase, IEquatable<SolutionLocation>
    {
        private string _folderStructure;
        public string FolderStructure
        {
            get
            {
                return this._folderStructure;
            }
            set
            {
                if (value != this._folderStructure)
                {
                    this._folderStructure = value;
                    this.InvokePropertyChanged(nameof(FolderStructure));
                }
            }
        }

        private string _project;
        public string Project
        {
            get
            {
                return this._project;
            }
            set
            {
                if (value != this._project)
                {
                    this._project = value;
                    this.InvokePropertyChanged(nameof(Project));
                }
            }
        }

        public string ToNamespace(string projectAssemblyName)
        {
            return $"{projectAssemblyName}.{string.Join(".", GetFolders())}";
        }

        public List<string> GetFolders()
        {
            return this.FolderStructure.Split('/').ToList();
        }

        public override string ToString()
        {
            return this._project + "/" + string.Join("/", this._folderStructure);
        }

        public bool Equals(SolutionLocation other)
        {
            if (other == null)
                return false;

            return this.ToString() == other.ToString();
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as SolutionLocation);
        }
    }

    public class SolutionLocationComparer : IEqualityComparer<SolutionLocation>
    {
        public bool Equals(SolutionLocation x, SolutionLocation y)
        {
            if (x == null || y == null)
                return false;

            return x.Equals(y);
        }

        public int GetHashCode(SolutionLocation obj)
        {
            if (obj == null)
                return 0;

            return obj.GetHashCode();
        }
    }
}
