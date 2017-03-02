using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DtoGenerator.Logic.Model
{
    public class EntityMetadata
    {
        public EntityMetadata()
        {
            this.Properties = new List<PropertyMetadata>();
        }

        public string BaseClassName { get; set; }
        public string BaseClassDtoName { get; set; }

        public string Name { get; set; }
        public string DtoName { get; set; }

        public string Namespace { get; set; }

        public List<PropertyMetadata> Properties { get; set; }
        public List<AttributeListSyntax> AttributesList { get; set; }

        internal EntityMetadata Clone()
        {
            return new EntityMetadata()
            {
                Name = this.Name,
                Namespace = this.Namespace,
                BaseClassName = this.BaseClassName,
                Properties = this.Properties.Select(p => p.Clone()).ToList(),
                AttributesList = this.AttributesList
            };
        }
    }
}
