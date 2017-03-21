using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Safran.ProjectName1.Model;
using System.ComponentModel.DataAnnotations;
using DtoGenerator.TestSolution.DAL.Dto.Infrastructure;

namespace Safran.ProjectName1.Business.DTO
{
    public class SampleTable2DTO
    {
        ////BCC/ BEGIN CUSTOM CODE SECTION 
        ////ECC/ END CUSTOM CODE SECTION 

        [Required]
        public int Id { get; set; }

        [StringLength(10)]
        [Required]
        public string Title { get; set; }

        [StringLength(200)]
        [Required]
        public string Description { get; set; }
    }

    public class SampleTable2Mapper : MapperBase<SampleTable2, SampleTable2DTO>
    {
        ////BCC/ BEGIN CUSTOM CODE SECTION 
        ////ECC/ END CUSTOM CODE SECTION 

        public override Expression<Func<SampleTable2, SampleTable2DTO>> SelectorExpression
        {
            get
            {
                return ((Expression<Func<SampleTable2, SampleTable2DTO>>)(p => new SampleTable2DTO()
                {
                    ////BCC/ BEGIN CUSTOM CODE SECTION 
                    ////ECC/ END CUSTOM CODE SECTION 
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                }));
            }
        }

        public override void MapToModel(SampleTable2DTO dto, SampleTable2 model)
        {
            ////BCC/ BEGIN CUSTOM CODE SECTION 
            ////ECC/ END CUSTOM CODE SECTION 
            model.Id = dto.Id;
            model.Title = dto.Title;
            model.Description = dto.Description;
        }
    }
}
