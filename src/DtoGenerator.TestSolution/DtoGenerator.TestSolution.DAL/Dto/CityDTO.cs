using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DtoGenerator.TestSolution.DAL.Infrastructure;
using DtoGenerator.TestSolution.Model.Entity;

namespace DtoGenerator.TestSolution.DAL.Dto
{
    public class CityDTO
    {
        public Guid UniqueId { get; set; }
        public string PostalCode { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }

        ////BCPS/ BEGIN CUSTOM PROPERTY SECTION 

        public string CustomProperty { get; set; }

        ////ECPS/ END CUSTOM PROPERTY SECTION
    }

    public class CityMapper : MapperBase<City, CityDTO>
    {
        public override Expression<Func<City, CityDTO>> SelectorExpression
        {
            get
            {
                return p => new CityDTO()
                {
                    UniqueId = p.UniqueId,
                    Name = p.Name,
                    DateCreated = p.DateCreated,
                    PostalCode = p.PostalCode,

                    ////BCSS/ BEGIN CUSTOM SELECTOR SECTION 

                    CustomProperty = p.Name + p.PostalCode,

                    ////ECSS/ END CUSTOM SELECTOR SECTION
                };
            }
        }

        public override void MapToModel(CityDTO dto, City model)
        {
            model.UniqueId = dto.UniqueId;
            model.PostalCode = dto.PostalCode;
            model.Name = dto.Name;
            model.DateCreated = dto.DateCreated;

            ////BCMS/ BEGIN CUSTOM MAP SECTION 

            model.Name += dto.CustomProperty;

            ////ECMS/ END CUSTOM MAP SECTION
        }
    }
}
