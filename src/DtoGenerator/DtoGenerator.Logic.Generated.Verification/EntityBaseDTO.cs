using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DtoGenerator.Logic.Generated.Verification
{
    public class EntityBaseDTO
    {
        public int Id { get; set; }
    }

    public class EntityBaseMapper : MapperBase<EntityBase, EntityBaseDTO>
    {

        public override Expression<Func<EntityBase, EntityBaseDTO>> SelectorExpression
        {
            get
            {
                return p => new EntityBaseDTO()
                {
                    Id = p.Id,
                };
            }
        }

        public override void MapToModel(EntityBaseDTO dto, EntityBase model)
        {
            model.Id = dto.Id;
        }
    }
}
