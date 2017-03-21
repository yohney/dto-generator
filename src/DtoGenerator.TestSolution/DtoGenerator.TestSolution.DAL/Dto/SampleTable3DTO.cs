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
    public class SampleTable3DTO
    {
        ////BCC/ BEGIN CUSTOM CODE SECTION 
        public ICollection<int> SampleTable1Ids { get { return SampleTable1?.Select(s => s.Id).ToList(); } set { SampleTable1 = value.Select(v => new SampleTable1DTO() { Id = v }).ToList(); } }
        public ICollection<SampleTable1DTO> SampleTable1 { get; set; }
        ////ECC/ END CUSTOM CODE SECTION 

        [Required]
        public int Id { get; set; }

        [StringLength(50)]
        [Required]
        public string Title { get; set; }

        [StringLength(200)]
        [Required]
        public string Description { get; set; }

        [Required]
        public short Value { get; set; }
    }

    public class SampleTable3Mapper : MapperBase<SampleTable3, SampleTable3DTO>
    {
        ////BCC/ BEGIN CUSTOM CODE SECTION 
        ////ECC/ END CUSTOM CODE SECTION 

        public override Expression<Func<SampleTable3, SampleTable3DTO>> SelectorExpression
        {
            get
            {
                return ((Expression<Func<SampleTable3, SampleTable3DTO>>)(p => new SampleTable3DTO()
                {
                    ////BCC/ BEGIN CUSTOM CODE SECTION 
                    SampleTable1= p.SampleTable1.Select(s => new SampleTable1DTO() { Id = s.Id, Title = s.Title, Description = s.Description }).ToList(),
                    ////ECC/ END CUSTOM CODE SECTION 
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    Value = p.Value,
                }));
            }
        }

        public override void MapToModel(SampleTable3DTO dto, SampleTable3 model)
        {
            ////BCC/ BEGIN CUSTOM CODE SECTION 
            if (dto.SampleTable1 != null) { model.SampleTable1 = dto.SampleTable1.Select(s => new SampleTable1() { Id = s.Id, Title = s.Title, Description = s.Description}).ToList(); }
            ////ECC/ END CUSTOM CODE SECTION 
            model.Id = dto.Id;
            model.Title = dto.Title;
            model.Description = dto.Description;
            model.Value = dto.Value;

        }
    }
}
