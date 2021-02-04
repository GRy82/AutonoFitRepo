using AutonoFit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.Contracts
{
    public interface IClientExerciseRepository : IRepositoryBase<ClientExercise>
    {
        void CreateClientExercise(ClientExercise clientExercise);
        Task<List<ClientExercise>> GetAllClientExercisesAsync(int clientId);
        Task<List<ClientExercise>> GetClientExerciseAsync(int exerciseId);
        void EditClientExercise(ClientExercise clientExercise);
        void DeleteClientExercise(ClientExercise clientExercise);
    }
}
