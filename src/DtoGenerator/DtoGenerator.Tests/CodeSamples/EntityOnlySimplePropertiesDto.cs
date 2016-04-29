using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Existing.Filled.Dto
{
    public class EntityOnlySimplePropertiesDTO
    {
        ////BCPS/ BEGIN CUSTOM PROPERTY SECTION 

        public int CustomProperty { get; set; }

        ////ECPS/ END CUSTOM PROPERTY SECTION
    }

    public class EntityOnlySimplePropertiesMapper : MapperBase<EntityOnlySimpleProperties, EntityOnlySimplePropertiesDTO>
    {
        public override Expression<Func<EntityOnlySimpleProperties, EntityOnlySimplePropertiesDTO>> SelectorExpression
        {
            get
            {
                return p => new EntityOnlySimplePropertiesDTO()
                {
                    ////BCSS/ BEGIN CUSTOM SELECTOR SECTION 

                    CustomProperty = p.CustomPropertyX

                    ////ECSS/ END CUSTOM SELECTOR SECTION
                };
            }
        }

        public override void MapToModel(EntityOnlySimplePropertiesDTO dto, EntityOnlySimpleProperties model)
        {
            ////BCMS/ BEGIN CUSTOM MAP SECTION 

            model.CustomProperty = dto.CustomProperty;

            ////ECMS/ END CUSTOM MAP SECTION
        }
    }
}
