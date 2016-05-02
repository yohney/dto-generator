using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Existing.Filled.Dto
{
    public class EntityOnlySimplePropertiesDTO
    {
        public int PreviouslyGeneratedProperty1 { get; set; }
        public string PreviouslyGeneratedProperty2 { get; set; }

        ////BCPS/ BEGIN CUSTOM PROPERTY SECTION 

        public int CustomProperty { get; set; }

        ////ECPS/ END CUSTOM PROPERTY SECTION

        public DateTime? PreviouslyGeneratedProperty3 { get; set; }
    }

    public class EntityOnlySimplePropertiesMapper : MapperBase<EntityOnlySimpleProperties, EntityOnlySimplePropertiesDTO>
    {
        public override Expression<Func<EntityOnlySimpleProperties, EntityOnlySimplePropertiesDTO>> SelectorExpression
        {
            get
            {
                return p => new EntityOnlySimplePropertiesDTO()
                {
                    PreviouslyGeneratedProperty1 = p.Test1,

                    ////BCSS/ BEGIN CUSTOM SELECTOR SECTION 

                    CustomProperty = p.CustomPropertyX,

                    ////ECSS/ END CUSTOM SELECTOR SECTION

                    PreviouslyGeneratedProperty2 = p.Test1,
                    PreviouslyGeneratedProperty3 = p.Test1,
                };
            }
        }

        public override void MapToModel(EntityOnlySimplePropertiesDTO dto, EntityOnlySimpleProperties model)
        {
            model.Property1 = dto.PreviouslyGeneratedProperty1;

            ////BCMS/ BEGIN CUSTOM MAP SECTION 

            model.CustomProperty = dto.CustomProperty;

            ////ECMS/ END CUSTOM MAP SECTION

            model.Property2 = dto.PreviouslyGeneratedProperty2;
        }
    }
}
