using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DtoGenerator.Tests.CodeSamples
{
    public class ComplexEntity
    {
        public string Name { get; set; }
        public DateTime? Date { get; set; }
        public string OtherString { get; set; }

        public OtherEntity Other { get; set; }

        public List<Something> List1 { get; set; }
        public virtual IEnumerable<Something> Enumerable2 { get; set; }
        public virtual ICollection<Something> Collection2 { get; set; }
    }
}
