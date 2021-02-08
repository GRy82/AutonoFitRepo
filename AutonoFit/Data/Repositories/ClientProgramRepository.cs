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
    public class ClientProgramRepository : RepositoryBase<ClientProgram>, IClientProgramRepository
    {
        private readonly object _repo;

        public ClientProgramRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {

        }
        public void CreateClientProgram(ClientProgram clientProgram) => Create(clientProgram);

        public async Task<List<ClientProgram>> GetAllClientProgramsAsync() =>
            await FindAll().ToListAsync();

        public async Task<ClientProgram> GetClientProgramAsync(int clientProgramId) =>
            await FindByCondition(c => c.ProgramId.Equals(clientProgramId)).FirstOrDefaultAsync();

        public void EditClientProgram(ClientProgram clientProgram) => Update(clientProgram);
        public void DeleteClientProgram(ClientProgram clientProgram) => Delete(clientProgram);
    }
}
