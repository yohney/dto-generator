using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 
{
    public class PersonDTO
{////BCC/ BEGIN CUSTOM CODE SECTION ////ECC/ END CUSTOM CODE SECTION public string FullName
{
    get;
    set;
}
public string CityPostalCode
{
    get;
    set;
}

    }

    public class PersonMapper : MapperBase<Person, PersonDTO>
{////BCC/ BEGIN CUSTOM CODE SECTION ////ECC/ END CUSTOM CODE SECTION         public override Expression<Func<Person, PersonDTO>> SelectorExpression
        {
            get
            {
                return p => new PersonDTO()
{////BCC/ BEGIN CUSTOM CODE SECTION ////ECC/ END CUSTOM CODE SECTION FullName=p.FullName
,CityPostalCode=p.City.PostalCode

                };
            }
        }

        public override void MapToModel(PersonDTO dto, Person model)
{////BCC/ BEGIN CUSTOM CODE SECTION ////ECC/ END CUSTOM CODE SECTION model.FullName = dto.FullName;

        }
    }
}
