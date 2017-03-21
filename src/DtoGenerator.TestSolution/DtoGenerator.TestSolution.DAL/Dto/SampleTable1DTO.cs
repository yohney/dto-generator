using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DtoGenerator.TestSolution.DAL.Dto.Infrastructure;
using Safran.ProjectName1.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Safran.ProjectName1.Business.DTO
{
    public class SampleTable1DTO
    {

        ////BCC/ BEGIN CUSTOM CODE SECTION 
        //Option : Generate linked objects and ID gettter 
        [Required]
        public int SampleTable2Id { get { return SampleTable2 != null ? SampleTable2.Id : 0; } set { SampleTable2 = new SampleTable2DTO() { Id = value }; } }
        public Nullable<int> SampleTable2_0_1Id { get { return SampleTable2_0_1?.Id; } set { SampleTable2_0_1 = (value == null) ? null : new SampleTable2DTO() { Id = value.Value }; } }
        public ICollection<int> SampleTable3Ids { get { return SampleTable3?.Select(s => s.Id).ToList(); } set { SampleTable3 = value.Select(v => new SampleTable3DTO() { Id = v }).ToList(); } }

        [Required]
        public SampleTable2DTO SampleTable2 { get; set; }

        public SampleTable2DTO SampleTable2_0_1 { get; set; }

        public ICollection<SampleTable3DTO> SampleTable3 { get; set; }

        int test = 3;

        public int GetInt()
        {
            return test;
        }
        ////ECC/ END CUSTOM CODE SECTION 

        [Required]
        public int Id { get; set; }

        [StringLength(10)]
        [Required]
        public string Title { get; set; }

        [StringLength(200)]
        [Required]
        public string Description { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> Date { get; set; }

    }

    public class SampleTable1Mapper : MapperBase<SampleTable1, SampleTable1DTO>
    {

        ////BCC/ BEGIN CUSTOM CODE SECTION 
        int test = 3;

        public int GetInt()
        {
            return test;
        }
        ////ECC/ END CUSTOM CODE SECTION 
        private SampleTable3Mapper _sampleTable3Mapper = new SampleTable3Mapper();

        public override Expression<Func<SampleTable1, SampleTable1DTO>> SelectorExpression
        {
            get
            {
                return ((Expression<Func<SampleTable1, SampleTable1DTO>>)(p => new SampleTable1DTO()
                {

                    ////BCC/ BEGIN CUSTOM CODE SECTION 
                    SampleTable2 = (p.SampleTable2 == null) ? null : new SampleTable2DTO() { Id = p.SampleTable2.Id, Title = p.SampleTable2.Title, Description = p.SampleTable2.Description },
                    SampleTable2_0_1 = (p.SampleTable2_0_1 == null) ? null : new SampleTable2DTO() { Id = p.SampleTable2_0_1.Id, Title = p.SampleTable2_0_1.Title, Description = p.SampleTable2_0_1.Description },
                    SampleTable3 = p.SampleTable3.Select(s => new SampleTable3DTO() { Id = s.Id, Title = s.Title, Description = s.Description, Value = s.Value }).ToList(),
                    ////ECC/ END CUSTOM CODE SECTION 
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    Date = p.Date,
                }));
            }
        }

        public override void MapToModel(SampleTable1DTO dto, SampleTable1 model)
        {

            ////BCC/ BEGIN CUSTOM CODE SECTION 
            //Option : Map linked object properties
            if (dto.SampleTable2 != null) { model.SampleTable2 = new SampleTable2() { Id = dto.SampleTable2.Id, Title = dto.SampleTable2.Title, Description = dto.SampleTable2.Description }; }
            if (dto.SampleTable2_0_1 != null) { model.SampleTable2_0_1 = new SampleTable2() { Id = dto.SampleTable2_0_1.Id, Title = dto.SampleTable2_0_1.Title, Description = dto.SampleTable2_0_1.Description }; }
            if (dto.SampleTable3 != null) { model.SampleTable3 = dto.SampleTable3.Select(s => new SampleTable3() { Id = s.Id, Title = s.Title, Description = s.Description, Value = s.Value }).ToList(); }
            ////ECC/ END CUSTOM CODE SECTION 
            model.Id = dto.Id;
            model.Title = dto.Title;
            model.Description = dto.Description;
            model.Date = dto.Date;
        }
    }
}
