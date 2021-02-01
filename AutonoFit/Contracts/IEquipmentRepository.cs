using AutonoFit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.Contracts
{
    public interface IEquipmentRepository : IRepositoryBase<Equipment>
    {
        void CreateEquipment(Equipment equipment);
        Task<List<Equipment>> GetAllEquipmentAsync();
        Task<Equipment> GetEquipmentAsync(int equipmentId);
        void EditEquipment(Equipment equipment);
        void DeleteEquipment(Equipment equipment);
    }
}
