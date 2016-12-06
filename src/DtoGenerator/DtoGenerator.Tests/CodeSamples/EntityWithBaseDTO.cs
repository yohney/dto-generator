using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Existing.Filled.Dto
{
    public class EntityWithBaseDTO : EntityBaseDTO
    {
        public string Name { get; set; }
        
        ////BCC/ BEGIN CUSTOM CODE SECTION 
        ////ECC/ END CUSTOM CODE SECTION
    }

    public class EntityWithBaseMapper : MapperBase<EntityWithBase, EntityWithBaseDTO>
    {
        ////BCC/ BEGIN CUSTOM CODE SECTION 

        ////ECC/ END CUSTOM CODE SECTION
        private EntityBaseMapper _entityBaseMapper = new EntityBaseMapper();
        public override Expression<Func<EntityWithBase, EntityWithBaseDTO>> SelectorExpression
        {
            get
            {
                return p => new EntityWithBaseDTO()
                {
                    Name = p.Name

                    ////BCC/ BEGIN CUSTOM CODE SECTION 
                    ////ECC/ END CUSTOM CODE SECTION
                };
            }
        }

        public override void MapToModel(EntityWithBaseDTO dto, EntityWithBase model)
        {
            model.Name = dto.Name;
            
            ////BCC/ BEGIN CUSTOM CODE SECTION 
            ////ECC/ END CUSTOM CODE SECTION
        }
    }
}
