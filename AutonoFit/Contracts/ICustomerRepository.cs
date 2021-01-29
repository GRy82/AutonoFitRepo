using AutonoFit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.Contracts
{
    public interface IClientRepository : IRepositoryBase<Client> //Class interface will contain methods unique to the model
    {
        void CreateCustomer(Client client);
        Task<List<Client>> GetAllCustomersAsync();
        Task<Client> GetCustomerAsync(int clientId);
        void EditCustomer(Client client); 
        void DeleteCustomer(Client client);
    }
}
