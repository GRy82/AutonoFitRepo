using AutonoFit.Models;
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

        public List<Goals> AvailableGoals { get; set; }

        public List<int> GoalIds { get; set; }

        public string BodySection { get; set; }

        public int Minutes { get; set; }


        public string ErrorMessage { get; set; }

    }
}
