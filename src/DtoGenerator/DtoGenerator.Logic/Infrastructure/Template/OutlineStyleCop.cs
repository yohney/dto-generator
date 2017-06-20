namespace #Namespace#
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using System.Threading.Tasks;

#pragma warning disable CS1591 // Missing XML Comment
#pragma warning disable SA1600 // Elements must be documented
#pragma warning disable SA1402 // File may only contain one single class
    public class #DTO# #Inheritance#
    {

    }

    public class #DTOAdjusted#Mapper : MapperBase<#Entity#, #DTO#>
    {
        public override Expression<Func<#Entity#, #DTO#>> SelectorExpression
        {
            get
            {
                return (Expression<Func<#Entity#, #DTO#>>)(p => new #DTO#()
                {
                    
                });
            }
        }

        public override void MapToModel(#DTO# dto, #Entity# model)
        {
        }
    }
#pragma warning restore CS1591 // Missing XML Comment
#pragma warning restore SA1600 // Elements must be documented
#pragma warning restore SA1402 // File may only contain one single class
}