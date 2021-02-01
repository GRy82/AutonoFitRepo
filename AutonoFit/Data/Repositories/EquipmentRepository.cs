using AutonoFit.Contracts;
using AutonoFit.Models;
using AutonoFit.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.Data.Repositories
{

    public class EquipmentRepository : RepositoryBase<Equipment>, IEquipmentRepository
    {
        private readonly object _repo;

        public EquipmentRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {

        }
        public void CreateEquipment(Equipment equipment) => Create(equipment);

        public async Task<List<Equipment>> GetAllEquipmentAsync() =>
            await FindAll().ToListAsync();

        public async Task<Equipment> GetEquipmentAsync(int equipmentId) =>
            await FindByCondition(c => c.EquipmentId.Equals(equipmentId)).FirstOrDefaultAsync();


        public void EditEquipment(Equipment equipment) => Update(equipment);
        public void DeleteEquipment(Equipment equipment) => Delete(equipment);
    }
}
