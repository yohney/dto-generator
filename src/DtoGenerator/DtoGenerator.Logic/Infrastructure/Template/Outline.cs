using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace #Namespace#
{
    public class #Entity#DTO
    {
        ////BCPS/ BEGIN CUSTOM PROPERTY SECTION 

        ////ECPS/ END CUSTOM PROPERTY SECTION
    }

    public class #Entity#Mapper : MapperBase<#Entity#, #Entity#DTO>
    {
        public override Expression<Func<#Entity#, #Entity#DTO>> SelectorExpression
        {
            get
            {
                return p => new #Entity#DTO()
                {
                    ////BCSS/ BEGIN CUSTOM SELECTOR SECTION 

                    ////ECSS/ END CUSTOM SELECTOR SECTION
                };
            }
        }

        public override void MapToModel(#Entity#DTO dto, #Entity# model)
        {
            ////BCMS/ BEGIN CUSTOM MAP SECTION 

            ////ECMS/ END CUSTOM MAP SECTION
        }
    }
}
