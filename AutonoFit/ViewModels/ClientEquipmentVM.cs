using AutonoFit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.ViewModels
{
    public class ClientEquipmentVM
    {
        public Client Client { get; set; }
        public List<Equipment> EquipmentList { get; set;} 
        public List<bool> EquipmentChecks { get; set; }
    }
}
