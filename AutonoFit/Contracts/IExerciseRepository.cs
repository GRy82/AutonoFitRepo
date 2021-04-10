using AutonoFit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.Contracts
{
    public interface IExerciseRepository : IRepositoryBase<Exercise>
    {
        void CreateExercise(Exercise exercise);
        Task<List<Exercise>> GetAllExercisesAsync(int clientId);
        Task<List<Exercise>> GetExerciseAsync(int exerciseId);
        Task<List<Exercise>> GetExerciseByWorkoutAsync(int workoutId);
        Task<List<Exercise>> GetExercisesByProgramAsync(int programId, int exerciseId);
        void EditExercise(Exercise exercise);
        void DeleteExercise(Exercise exercise);
    }
}
