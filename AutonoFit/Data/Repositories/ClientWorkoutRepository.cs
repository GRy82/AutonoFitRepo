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
    public class ClientWorkoutRepository : RepositoryBase<ClientWorkout>, IClientWorkoutRepository
    {
        private readonly object _repo;

        public ClientWorkoutRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {

        }
        public void CreateClientWorkout(ClientWorkout clientWorkout) => Create(clientWorkout);

        public async Task<List<ClientWorkout>> GetAllClientWorkoutsAsync(int clientId) =>
            await FindByCondition(c => c.ClientId.Equals(clientId)).ToListAsync();

        public async Task<ClientWorkout> GetClientWorkoutAsync(int clientWorkoutId) =>
            await FindByCondition(c => c.Id.Equals(clientWorkoutId)).FirstOrDefaultAsync();


        public void EditClientWorkout(ClientWorkout clientWorkout) => Update(clientWorkout);
        public void DeleteClientWorkout(ClientWorkout clientWorkout) => Delete(clientWorkout);

    }
}
