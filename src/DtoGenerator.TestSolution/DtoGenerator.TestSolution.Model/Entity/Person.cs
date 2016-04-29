using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DtoGenerator.TestSolution.Model.Infrastructure;

namespace DtoGenerator.TestSolution.Model.Entity
{
    public class Person : EntityBase
    {
        public string FullName { get; set; }

        [ForeignKey("City")]
        public int CityId { get; set; }
        public City City { get; set; }

        public virtual ICollection<PersonContact> Contacts { get; set; }
    }
}
