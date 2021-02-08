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
    public class ClientWeekRepository : RepositoryBase<ClientWeek>, IClientWeekRepository
    {
        private readonly object _repo;

        public ClientWeekRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {

        }
        public void CreateClientWeek(ClientWeek clientWeek) => Create(clientWeek);

        public async Task<List<ClientWeek>> GetAllClientWeeksAsync(int clientProgramId) =>
            await FindByCondition(c => c.ProgramId.Equals(clientProgramId)).ToListAsync();

        public async Task<ClientWeek> GetClientWeekAsync(int clientWeekId) =>
            await FindByCondition(c => c.Id.Equals(clientWeekId)).FirstOrDefaultAsync();

        public void EditClientWeek(ClientWeek clientWeek) => Update(clientWeek);
        public void DeleteClientWeek(ClientWeek clientWeek) => Delete(clientWeek);
    }
}
