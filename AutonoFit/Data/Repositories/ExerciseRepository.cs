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
    public class ExerciseRepository : RepositoryBase<Exercise>, IExerciseRepository
    {
        private readonly object _repo;

        public ExerciseRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {

        }

        public void CreateExercise(Exercise exercise) => Create(exercise);

        public async Task<List<Exercise>> GetAllExercisesAsync(int clientId) =>
            await FindByCondition(c => c.ClientId.Equals(clientId)).ToListAsync();

        public async Task<List<Exercise>> GetExerciseAsync(int exerciseId) =>
            await FindByCondition(c => c.exerciseId.Equals(exerciseId)).ToListAsync();

        public async Task<List<Exercise>> GetExerciseByWorkoutAsync(int workoutId) =>
            await FindByCondition(c => c.WorkoutId.Equals(workoutId)).ToListAsync();

        public async Task<List<Exercise>> GetExercisesByProgramAsync(int programId) =>
            await FindByCondition(c => c.ProgramId.Equals(programId)).ToListAsync();

        public async Task<List<Exercise>> GetSameExercisesByProgramGoalAsync(int programId, int exerciseId, int goalId)
        {
            return await FindByCondition(c => c.ProgramId.Equals(programId)
                                           && c.GoalId.Equals(goalId)
                                           && c.exerciseId.Equals(exerciseId))
                                               .ToListAsync();
            
        }

        public async Task<List<Exercise>> GetPreviousExercisesAsync(int programId, int goalId)
        {
            return await FindByCondition(c => c.ProgramId.Equals(programId)
                                                   && c.GoalId.Equals(goalId))
                                                       .ToListAsync();
        }

        public void EditExercise(Exercise exercise) => Update(exercise);
        public void DeleteExercise(Exercise exercise) => Delete(exercise);


    }
}
