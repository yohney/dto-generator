using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DtoGenerator.TestSolution.DAL.Dto.Infrastructure;
using DtoGenerator.TestSolution.Model.Entity;

namespace DtoGenerator.TestSolution.DAL.Dto
{
    public class CityDTO
    {
        public Guid UniqueId { get; set; }
        public string PostalCode { get; set; }
        public string Name { get; set; }
        public string CountryCode { get; set; }
        public DateTime DateCreated { get; set; }

        ////BCC/ BEGIN CUSTOM CODE SECTION

        public string CustomProperty { get; set; }

        ////ECC/ END CUSTOM CODE SECTION
    }

    public class CityMapper : MapperBase<City, CityDTO>
    {
        public override Expression<Func<Role, CityDTO>> CustomSelectorExpression
        {
            get
            {
                return p => new CityDTO()
                {
                    Name = "City from a Role",
                };
            }
        }

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
                    CountryCode = p.Country != null ? p.Country.Code : null,

                    ////BCC/ BEGIN CUSTOM CODE SECTION

                    CustomProperty = p.Name + p.PostalCode,

                    ////ECC/ END CUSTOM CODE SECTION
                };
            }
        }

        public override void MapToModel(CityDTO dto, City model)
        {
            model.UniqueId = dto.UniqueId;
            model.PostalCode = dto.PostalCode;
            model.Name = dto.Name;
            model.DateCreated = dto.DateCreated;

            ////BCC/ BEGIN CUSTOM CODE SECTION

            model.Name += dto.CustomProperty;

            ////ECC/ END CUSTOM CODE SECTION
        }
    }
}
