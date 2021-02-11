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
    public class ClientExerciseRepository : RepositoryBase<ClientExercise>, IClientExerciseRepository
    {
        private readonly object _repo;

        public ClientExerciseRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {

        }

        public void CreateClientExercise(ClientExercise clientExercise) => Create(clientExercise);

        public async Task<List<ClientExercise>> GetAllClientExercisesAsync(int clientId) =>
            await FindByCondition(c => c.ClientId.Equals(clientId)).ToListAsync();

        public async Task<List<ClientExercise>> GetClientExerciseAsync(int exerciseId) =>
            await FindByCondition(c => c.ExerciseId.Equals(exerciseId)).ToListAsync();

        public async Task<List<ClientExercise>> GetClientExerciseByWorkoutAsync(int workoutId) =>
            await FindByCondition(c => c.WorkoutId.Equals(workoutId)).ToListAsync();

        public async Task<List<ClientExercise>> GetClientExercisesByProgramAsync(int programId, int exerciseId)
        {
            List<ClientExercise> programExercises = await FindByCondition(c => c.ProgramId.Equals(programId)).ToListAsync();
            List<ClientExercise> exercises = new List<ClientExercise> { };
            foreach (ClientExercise exercise in programExercises)
            {
                if (exercise.ExerciseId == exerciseId)
                {
                    exercises.Add(exercise);
                }
            }
            return exercises;
        }

        public void EditClientExercise(ClientExercise clientExercise) => Update(clientExercise);
        public void DeleteClientExercise(ClientExercise clientExercise) => Delete(clientExercise);


    }
}
