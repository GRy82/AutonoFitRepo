using AutonoFit.Contracts;
using AutonoFit.Data;
using AutonoFit.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.Repositories
{
    public class ClientRepository : RepositoryBase<Client>, IClientRepository
    {
        private readonly object _repo;

        public ClientRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {

        }
        public void CreateClient(Client client) => Create(client);

        public async Task<List<Client>> GetAllClientsAsync() =>
            await FindAll().ToListAsync();

        public async Task<Client> GetClientAsync(int clientId) =>
            await FindByCondition(c => c.ClientId.Equals(clientId)).FirstOrDefaultAsync();

        public void EditClient(Client client) => Update(client);
        public void DeleteClient(Client client) => Delete(client);
        


    }
}
