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

        public async Task<List<ClientWorkout>> GetAllWorkoutsByWeekAsync(int weekId) =>
            await FindByCondition(c => c.WeekId.Equals(weekId)).ToListAsync();


        //Find workouts >= 1 day old and not tied to a week/program. (For eventual deletion)
        public async Task<List<ClientWorkout>> GetOldWorkoutsAsync(int clientId)
        {
            List<ClientWorkout> pastWorkouts = await GetAllClientWorkoutsAsync(clientId);
            List<ClientWorkout> workoutsToDelete = new List<ClientWorkout> { };
            foreach (ClientWorkout workout in pastWorkouts)
            {
                if (workout.WeekId == null && Convert.ToDateTime(workout.DatePerformed).CompareTo(DateTime.Now.AddDays(-1)) <= 0)
                {
                    workoutsToDelete.Add(workout);
                }
            }

            return workoutsToDelete;
        }


        public void EditClientWorkout(ClientWorkout clientWorkout) => Update(clientWorkout);
        public void DeleteClientWorkout(ClientWorkout clientWorkout) => Delete(clientWorkout);

    }
}
