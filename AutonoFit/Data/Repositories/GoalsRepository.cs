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
    public class GoalsRepository : RepositoryBase<Goals>, IGoalsRepository
    {
        private readonly object _repo;

        public GoalsRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {

        }
        public void CreateGoals(Goals goals) => Create(goals);

        public async Task<List<Goals>> GetAllGoalsAsync() =>
            await FindAll().ToListAsync();

        public async Task<Goals> GetGoalsAsync(int goalId) =>
            await FindByCondition(c => c.GoalId.Equals(goalId)).FirstOrDefaultAsync();

        public void EditGoals(Goals goals) => Update(goals);
        public void DeleteGoals(Goals goals) => Delete(goals);
    }
}
