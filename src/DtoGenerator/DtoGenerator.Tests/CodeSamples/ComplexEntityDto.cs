using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Existing.Filled.Dto
{
    public class ComplexEntityDTO
    {
        public string Name { get; set; }
        public DateTime? Date { get; set; }
        public string OtherString { get; set; }

        public string OtherEntityName { get; set; }

        public IEnumerable<Something1> Somethings1 { get; set; }
        public IEnumerable<Something2> Somethings2 { get; set; }
        public IEnumerable<Something3> Somethings3 { get; set; }

        ////BCPS/ BEGIN CUSTOM PROPERTY SECTION 

        public int CustomProperty { get; set; }

        ////ECPS/ END CUSTOM PROPERTY SECTION
    }

    public class ComplexEntityMapper : MapperBase<ComplexEntity, ComplexEntityDTO>
    {
        private Something1Mapper _something1Mapper = new Something1Mapper();
        private Something2Mapper _something2Mapper = new Something2Mapper();
        private Something3Mapper _something3Mapper = new Something3Mapper();

        public override Expression<Func<ComplexEntity, ComplexEntityDTO>> SelectorExpression
        {
            get
            {
                return p => new ComplexEntityDTO()
                {
                    Name = p.Name,
                    Date = p.Date,
                    OtherEntityName = p.OtherEntity.Name,
                    Somethings1 = p.Somethings1.Select(this._something1Mapper.SelectorExpression),
                    Somethings2 = p.Somethings2.Select(this._something2Mapper.SelectorExpression),
                    Somethings3 = p.Somethings3.Select(this._something3Mapper.SelectorExpression),

                    ////BCSS/ BEGIN CUSTOM SELECTOR SECTION 

                    CustomProperty = p.CustomProperty,

                    ////ECSS/ END CUSTOM SELECTOR SECTION
                };
            }
        }

        public override void MapToModel(EntityOnlySimplePropertiesDTO dto, EntityOnlySimpleProperties model)
        {
            model.Name = dto.Name;
            model.Date = dto.Date;
            model.OtherString = dto.OtherString;

            ////BCMS/ BEGIN CUSTOM MAP SECTION 

            model.CustomProperty = dto.CustomProperty;

            ////ECMS/ END CUSTOM MAP SECTION
        }
    }
}
