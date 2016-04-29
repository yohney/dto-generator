using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DtoGenerator.TestSolution.Model.Infrastructure;

namespace DtoGenerator.TestSolution.Model.Entity
{
    public class PersonContact : EntityBase
    {
        public string Value { get; set; }

        public ContactType Type { get; set; }

        public int PersonId { get; set; }
        public Person Person { get; set; }
    }
}
