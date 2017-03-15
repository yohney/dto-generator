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
    public class ComputerDTO
    {
        ////BCC/ BEGIN CUSTOM CODE SECTION 
        private int _customField;

        public void SomeMethod()
        {

        }

        public ComputerDTO()
        {

        }

        public int Xy { get; set; }

        public void Something()
        {

        }

        public string Xy2 { get; set; }
        public void EndMethod()
        {

        }
        ////ECC/ END CUSTOM CODE SECTION 
        public string Name { get; set; }
        public int Cpus { get; set; }
    }

    public class ComputerMapper : MapperBase<Computer, ComputerDTO>
    {
        ////BCC/ BEGIN CUSTOM CODE SECTION 
        public Expression<Func<City, ComputerDTO>> SelectorExpressionFromCity
        {
            get
            {
                return ((Expression<Func<City, ComputerDTO>>)(p => new ComputerDTO()
                {
                    Name = "None"

                }));
            }
        }

        public void Test()
        {

        }
        ////ECC/ END CUSTOM CODE SECTION 

        public override Expression<Func<Computer, ComputerDTO>> SelectorExpression
        {
            get
            {
                return ((Expression<Func<Computer, ComputerDTO>>)(p => new ComputerDTO()
                {
                    ////BCC/ BEGIN CUSTOM CODE SECTION 
                    Xy = 77,
                    // this is my extra custom comment
                    Xy2 = null,
                    // another comment
                    ////ECC/ END CUSTOM CODE SECTION 
                    Name = p.Name,

                }));
            }
        }

        public override void MapToModel(ComputerDTO dto, Computer model)
        {

            ////BCC/ BEGIN CUSTOM CODE SECTION 
            var x = 0;
            x++;
            model.Cpus = x;
            ////ECC/ END CUSTOM CODE SECTION 
            model.Name = dto.Name;
            model.Cpus = dto.Cpus;
        }
    }
}
