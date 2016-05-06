using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DtoGenerator.Logic.Generated.Verification
{
    public class PersonDTO : EntityBaseDTO
    {
        public string FullName { get; set; }
    }

    public class PersonMapper : MapperBase<Person, PersonDTO>
    {
        private EntityBaseMapper _entityBaseMapper = new EntityBaseMapper();

        public override Expression<Func<Person, PersonDTO>> SelectorExpression
        {
            get
            {
                return ((Expression<Func<Person, PersonDTO>>)(p => new PersonDTO()
                {
                    FullName = p.FullName
                })).MergeWith(this._entityBaseMapper.SelectorExpression);
            }
        }

        public override void MapToModel(PersonDTO dto, Person model)
        {
            model.FullName = dto.FullName;

            this._entityBaseMapper.MapToModel(dto, model);
        }
    }
}
