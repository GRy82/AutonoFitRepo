using AutonoFit.Classes;
using AutonoFit.Models;
using AutonoFit.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.ViewModels
{
    public class ProgramWorkoutVM
    {
        public FitnessParameters FitnessParameters { get; set; }

        public List<ClientExercise> ClientExercises { get; set; }

        public List<Exercise> Exercises { get; set; }

        public ClientWorkout ClientWorkout { get; set; }

        public List<int> RPEs { get; set; }

        public int CardioRPE { get; set; }

    }
}
