using AutonoFit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.Contracts
{
    public interface IClientRepository : IRepositoryBase<Client> //Class interface will contain methods unique to the model
    {
        void CreateClient(Client client);
        Task<List<Client>> GetAllClientsAsync();
        Task<Client> GetClientAsync(int clientId);
        Task<Client> GetClientAsync(string userId);
        void EditClient(Client client); 
        void DeleteClient(Client client);
    }
}
