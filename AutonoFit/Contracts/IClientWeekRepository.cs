using AutonoFit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.Contracts
{
    public interface IClientWeekRepository : IRepositoryBase<ClientWeek>
    {
        void CreateClientWeek(ClientWeek clientWeek);
        Task<List<ClientWeek>> GetAllClientWeeksAsync(int clientProgramId);
        Task<ClientWeek> GetClientWeekAsync(int clientWeekId);
        void EditClientWeek(ClientWeek clientWeek);
        void DeleteClientWeek(ClientWeek clientWeek);
    }
}
