using AutonoFit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.Contracts
{
    public interface IClientEquipmentRepository : IRepositoryBase<ClientEquipment> //Class interface will contain methods unique to the model
    {
        void CreateClientEquipment(ClientEquipment clientEquipment);
        Task<List<ClientEquipment>> GetAllClientEquipmentAsync(int clientId);
        Task<ClientEquipment> GetClientEquipmentAsync(int equipmentId);
        void EditClientEquipment(ClientEquipment clientEquipment);
        void DeleteClientEquipment(ClientEquipment clientEquipment);
    
    }
}
