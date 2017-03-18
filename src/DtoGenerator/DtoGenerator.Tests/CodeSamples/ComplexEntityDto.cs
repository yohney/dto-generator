using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Existing.Filled.Dto
{
    public class ComplexEntityDTO
    {
        ////BCC/ BEGIN CUSTOM CODE SECTION 
        // Some custom property leading comment - should not dissapear
        public int CustomProperty { get; set; }

        public int SomeOtherCustomProperty
        {
            get
            {
                if (string.IsNullOrWhiteSpace(OtherString))
                    return new int();

                return new List<string>() { this.OtherString }.Count();
            }
        }

        ////ECC/ END CUSTOM CODE SECTION

        public string Name { get; set; }
        public DateTime? Date { get; set; }
        public string OtherString { get; set; }

        public string OtherEntityName { get; set; }

        public IEnumerable<Something1> Somethings1 { get; set; }
        public IEnumerable<Something2> Somethings2 { get; set; }
        public IEnumerable<Something3> Somethings3 { get; set; }

        
    }

    public class ComplexEntityMapper : MapperBase<ComplexEntity, ComplexEntityDTO>
    {
        private Something1Mapper _something1Mapper = new Something1Mapper();
        private Something2Mapper _something2Mapper = new Something2Mapper();
        private Something3Mapper _something3Mapper = new Something3Mapper();

        ////BCC/ BEGIN CUSTOM CODE SECTION 

        ////ECC/ END CUSTOM CODE SECTION

        public override Expression<Func<ComplexEntity, ComplexEntityDTO>> SelectorExpression
        {
            get
            {
                return p => new ComplexEntityDTO()
                {
                    Name = p.Name,
                    Date = p.Date,
                    OtherEntityName = p.OtherEntity != null && p.OtherEntity.Xyz != null && p.OtherEntity.Xyz.Cdf != null ? p.OtherEntity.Name : null,
                    OtherEntityName = p.OtherEntity != null ? p.OtherEntity.Name : default(string),
                    Somethings1 = p.Somethings1.Select(this._something1Mapper.SelectorExpression),
                    Somethings2 = p.Somethings2.Select(this._something2Mapper.SelectorExpression),
                    Somethings3 = p.Somethings3.Select(this._something3Mapper.SelectorExpression),

                    ////BCC/ BEGIN CUSTOM CODE SECTION 
                    
                    CustomProperty = p.CustomProperty,

                    ////ECC/ END CUSTOM CODE SECTION
                };
            }
        }

        public override void MapToModel(EntityOnlySimplePropertiesDTO dto, EntityOnlySimpleProperties model)
        {
            model.Name = dto.Name;
            model.Date = dto.Date;
            model.OtherString = dto.OtherString;

            ////BCC/ BEGIN CUSTOM CODE SECTION 
            // Some custom property 2 leading comment - should not dissapear
            model.CustomProperty = dto.CustomProperty;

            var t = new ComplexEntityDTO()
            {
                CustomProperty = ""
            };

            ////ECC/ END CUSTOM CODE SECTION
        }
    }
}
