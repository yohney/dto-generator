using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace #Namespace#
{
    public class #Entity#DTO #Inheritance#
    {

    }

    public class #Entity#Mapper : MapperBase<#Entity#, #Entity#DTO>
    {
        public override Expression<Func<#Entity#, #Entity#DTO>> SelectorExpression
        {
            get
            {
                return ((Expression<Func<#Entity#, #Entity#DTO>>)(p => new #Entity#DTO()
                {
                    
                }));
            }
        }

        public override void MapToModel(#Entity#DTO dto, #Entity# model)
        {

        }
    }
}
