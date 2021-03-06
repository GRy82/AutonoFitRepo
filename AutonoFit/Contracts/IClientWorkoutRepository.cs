﻿using AutonoFit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.Contracts
{
    public interface IClientWorkoutRepository : IRepositoryBase<ClientWorkout>
    {
        void CreateClientWorkout(ClientWorkout clientWorkout);
        Task<List<ClientWorkout>> GetAllClientWorkoutsAsync(int clientId);
        Task<ClientWorkout> GetClientWorkoutAsync(int workoutId);

        Task<List<ClientWorkout>> GetAllWorkoutsByProgramAsync(int programId);

        void EditClientWorkout(ClientWorkout clientWorkout);
        void DeleteClientWorkout(ClientWorkout clientWorkout);
    }
}
