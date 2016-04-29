using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DtoGenerator.Tests.CodeSamples
{
    public class EntityWithCollectionProperties
    {
        public List<Something> List1 { get; set; }

        public IEnumerable<Something> Enumerable1 { get; set; }

        public ICollection<Something> Collection1 { get; set; }

        public virtual List<Something> List2 { get; set; }

        public virtual IEnumerable<Something> Enumerable2 { get; set; }

        public virtual ICollection<Something> Collection2 { get; set; }
    }
}
