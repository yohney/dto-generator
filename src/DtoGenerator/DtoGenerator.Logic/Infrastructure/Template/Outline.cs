using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace #Namespace#
{
    public class #Entity#DTO
    {

    }

    public class #Entity#Mapper : MapperBase<#Entity#, #Entity#DTO>
    {
        public override Expression<Func<#Entity#, #Entity#DTO>> SelectorExpression
        {
            get
            {
                return p => new #Entity#DTO()
                {

                };
            }
        }

        public override void MapToModel(#Entity#DTO dto, #Entity# model)
        {

        }
    }
}
