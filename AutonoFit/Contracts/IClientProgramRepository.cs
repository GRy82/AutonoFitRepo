using AutonoFit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.Contracts
{
    public interface IClientProgramRepository : IRepositoryBase<ClientProgram>
    {
        void CreateClientProgram(ClientProgram clientProgram);
        Task<List<ClientProgram>> GetAllClientProgramsAsync(int clientId);
        Task<ClientProgram> GetClientProgramAsync(int clientProgramId);
        void EditClientProgram(ClientProgram clientProgram);
        void DeleteClientProgram(ClientProgram clientProgram);
    }
}
