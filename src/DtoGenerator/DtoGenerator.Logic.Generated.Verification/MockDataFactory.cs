using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DtoGenerator.Logic.Generated.Verification
{
    public static class MockDataFactory
    {
        public static List<Person> GetPersons()
        {
            return new List<Person>()
            {
                new Person()
                {
                    FullName = "Person 1",
                    Id = 1
                },
                new Person()
                {
                    FullName = "Person 2",
                    Id = 2
                }
            };
        }
    }
}
