using AutonoFit.Models;
using AutonoFit.Services;
using AutonoFit.StaticClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.ViewModels
{
    public class SingleWorkoutVM
    {
        public Client Client { get; set; }

        public ClientWorkout Workout { get; set; }

        public List<ClientEquipment> Equipment { get; set; }

        public List<Result> Exercises { get; set; }

        public List<Goals> AvailableGoals { get; set; }

        public FitnessDictionary fitnessDictionary { get; set; }

        public List<int> GoalIds { get; set; }

        public string BodySection { get; set; }

        public int Minutes { get; set; }

        public int MileMinutes { get; set; }

        public int MileSeconds { get; set; }

        public string ErrorMessage { get; set; }

    }
}
