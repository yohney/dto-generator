using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DtoGenerator.TestSolution.Model.Infrastructure
{
    public abstract class EntityBase
    {
        public virtual int Id { get; set; }

        public Guid UniqueId { get; set; }

        public DateTime DateCreated { get; set; }
    }
}
