using AutonoFit.Classes;
using AutonoFit.Models;
using AutonoFit.Services;
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

        public List<Exercise> Exercises { get; set; }

        public List<Goals> AvailableGoals { get; set; }

        public LiftingComponent LiftingComponent { get; set; }

        public CardioComponent CardioComponent { get; set; }

        public List<int> GoalIds { get; set; }

        public string BodySection { get; set; }

        public int Minutes { get; set; }

        public int MileMinutes { get; set; }

        public int MileSeconds { get; set; }

        public string ErrorMessage { get; set; }

        public bool DiscourageHighIntensity { get; set; }

    }
}
