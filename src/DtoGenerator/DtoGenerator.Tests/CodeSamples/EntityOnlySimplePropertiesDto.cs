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

        ////BCC/ BEGIN CUSTOM CODE SECTION 

        public int CustomProperty { get; set; }

        ////ECC/ END CUSTOM CODE SECTION

        public DateTime? PreviouslyGeneratedProperty3 { get; set; }
    }

    public class EntityOnlySimplePropertiesMapper : MapperBase<EntityOnlySimpleProperties, EntityOnlySimplePropertiesDTO>
    {
        ////BCC/ BEGIN CUSTOM CODE SECTION 

        public int Test()
        {
            return 0;
        }

        ////ECC/ END CUSTOM CODE SECTION

        public override Expression<Func<EntityOnlySimpleProperties, EntityOnlySimplePropertiesDTO>> SelectorExpression
        {
            get
            {
                return p => new EntityOnlySimplePropertiesDTO()
                {
                    PreviouslyGeneratedProperty1 = p.Test1,

                    ////BCC/ BEGIN CUSTOM CODE SECTION 

                    CustomProperty = p.CustomPropertyX,

                    ////ECC/ END CUSTOM CODE SECTION

                    PreviouslyGeneratedProperty2 = p.Test1,
                    PreviouslyGeneratedProperty3 = p.Test1,
                };
            }
        }

        public override void MapToModel(EntityOnlySimplePropertiesDTO dto, EntityOnlySimpleProperties model)
        {
            model.Property1 = dto.PreviouslyGeneratedProperty1;

            ////BCC/ BEGIN CUSTOM CODE SECTION 

            model.CustomProperty = dto.CustomProperty;

            ////ECC/ END CUSTOM CODE SECTION

            model.Property2 = dto.PreviouslyGeneratedProperty2;
        }
    }
}
