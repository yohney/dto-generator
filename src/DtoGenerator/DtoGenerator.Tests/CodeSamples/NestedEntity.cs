using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DtoGenerator.Tests.CodeSamples
{
    public class NestedEntity
    {
        public string Name { get; set; }

        public Nested RelatedNestedEntity { get; set; }
        
        public class Nested
        {
            public string Data1 { get; set; }
        }
    }
}
