﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.Models
{
    public class Equipment
    {
        [Key]
        public int EquipmentId { get; set; }
        public string Name { get; set; }


    }
}
