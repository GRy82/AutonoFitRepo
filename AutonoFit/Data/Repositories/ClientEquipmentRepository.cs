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
    public class ClientEquipmentRepository: RepositoryBase<ClientEquipment>, IClientEquipmentRepository
    {
        private readonly object _repo;

        public ClientEquipmentRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {

        }
        public void CreateClientEquipment(ClientEquipment clientEquipment) => Create(clientEquipment);

        public async Task<List<ClientEquipment>> GetAllClientEquipmentAsync() =>
            await FindAll().ToListAsync();

        public async Task<List<ClientEquipment>> GetClientEquipmentAsync(int clientId) =>
            await FindByCondition(c => c.ClientId.Equals(clientId)).ToListAsync();

        public void EditClientEquipment(ClientEquipment clientEquipment) => Update(clientEquipment);
        public void DeleteClientEquipment(ClientEquipment clientEquipment) => Delete(clientEquipment);

    }
}
