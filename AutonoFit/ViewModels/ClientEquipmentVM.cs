using AutonoFit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.ViewModels
{
    public class ClientEquipmentVM
    {
        Client client { get; set; }
        List<ClientEquipment> clientEquipment { get; set; }
    }
}
