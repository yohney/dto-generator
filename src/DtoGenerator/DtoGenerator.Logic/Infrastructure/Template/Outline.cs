using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace #Namespace#
{
    public class #DTO# #Inheritance#
    {

    }

    public class #DTO#Mapper : MapperBase<#Entity#, #DTO#>
    {
        public override Expression<Func<#Entity#, #DTO#>> SelectorExpression
        {
            get
            {
                return ((Expression<Func<#Entity#, #DTO#>>)(p => new #DTO#()
                {
                    
                }));
            }
        }

        public override void MapToModel(#DTO# dto, #Entity# model)
        {

        }
    }
}
