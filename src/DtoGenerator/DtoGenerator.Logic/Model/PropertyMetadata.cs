using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;

namespace DtoGenerator.Logic.Model
{
    public class PropertyMetadata
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsSimpleProperty { get; set; }

        public string RelatedEntityName { get; set; }
        public bool IsRelation { get; set; }
        public bool IsCollection { get; set; }
        public bool IsInherited { get; set; }

        public EntityMetadata RelationMetadata { get; set; }
        public List<AttributeListSyntax> AttributesList { get; set; }

        internal PropertyMetadata Clone()
        {
            return new PropertyMetadata()
            {
                Name = this.Name,
                Type = this.Type,
                IsSimpleProperty = this.IsSimpleProperty,
                RelatedEntityName = this.RelatedEntityName,
                IsRelation = this.IsRelation,
                IsCollection = this.IsCollection,
                RelationMetadata = this.RelationMetadata?.Clone(),
                AttributesList = this.AttributesList
            };
        }
    }
}
