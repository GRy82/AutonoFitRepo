using AutonoFit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.Contracts
{
    public interface IGoalsRepository : IRepositoryBase<Goals>
    {
        void CreateGoals(Goals goals);
        Task<List<Goals>> GetAllGoalsAsync();
        Task<Goals> GetGoalsAsync(int clientId);
        void EditGoals(Goals goals);
        void DeleteGoals(Goals goals);
    }
}
