using AutonoFit.Contracts;
using AutonoFit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.StaticClasses
{
    public static class ProgramModule
    {
        private static readonly IRepositoryWrapper _repo;

        public static async Task<bool> ProgramNameTaken(string programName, int clientId)
        {
            List<ClientProgram> programs = await _repo.ClientProgram.GetAllClientProgramsAsync(clientId);
            foreach (ClientProgram program in programs)
            {
                if (program.ProgramName == programName)
                {
                    return true;
                }
            }

            return false;
        }

    }
}
