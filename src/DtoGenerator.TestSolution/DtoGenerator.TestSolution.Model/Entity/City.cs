﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DtoGenerator.TestSolution.Model.Infrastructure;

namespace DtoGenerator.TestSolution.Model.Entity
{
    public class City : EntityBase
    {
        public Guid UniqueId { get; set; }
        public DateTime DateCreated { get; set; }
        public string PostalCode { get; set; }

        public string Name { get; set; }

        public Country Country { get; set; }
    }
}
