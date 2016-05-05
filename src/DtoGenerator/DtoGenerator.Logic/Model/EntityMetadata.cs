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

        public string Name { get; set; }
        public string DtoName => Name + "DTO";

        public string Namespace { get; set; }

        public List<PropertyMetadata> Properties { get; set; }

        internal EntityMetadata Clone()
        {
            return new EntityMetadata()
            {
                Name = this.Name,
                Namespace = this.Namespace,
                Properties = this.Properties.Select(p => p.Clone()).ToList()
            };
        }
    }
}
