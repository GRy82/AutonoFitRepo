using AutonoFit.Models;
using AutonoFit.Services;
using AutonoFit.StaticClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.ViewModels
{
    public class ProgramWorkoutVM
    {
        public List<FitnessDictionary> FitnessDictionary { get; set; }

        public List<ClientExercise> ClientExercises { get; set; }

        public List<Result> Exercises { get; set; }

    }
}
