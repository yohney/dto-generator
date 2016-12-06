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
    public class RoleDTO : EntityBaseDTO
    {
        ////BCC/ BEGIN CUSTOM CODE SECTION 
        ////ECC/ END CUSTOM CODE SECTION 
        public string Name { get; set; }
    }

    public class RoleMapper : MapperBase<Role, RoleDTO>
    {
        ////BCC/ BEGIN CUSTOM CODE SECTION 
        ////ECC/ END CUSTOM CODE SECTION 
        private EntityBaseMapper _entityBaseMapper = new EntityBaseMapper();
        public override Expression<Func<Role, RoleDTO>> SelectorExpression
        {
            get
            {
                return ((Expression<Func<Role, RoleDTO>>)(p => new RoleDTO()
                {
                    ////BCC/ BEGIN CUSTOM CODE SECTION 
                    ////ECC/ END CUSTOM CODE SECTION 
                    Name = p.Name,

                })).MergeWith(this._entityBaseMapper.SelectorExpression);
            }
        }

        public override void MapToModel(RoleDTO dto, Role model)
        {
            ////BCC/ BEGIN CUSTOM CODE SECTION 
            ////ECC/ END CUSTOM CODE SECTION 
            model.Name = dto.Name;
            this._entityBaseMapper.MapToModel(dto, model);
        }
    }
}
