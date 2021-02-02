using AutonoFit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.ViewModels
{
    public class WorkoutVM
    {
        public Client Client { get; set; }

        public ClientWorkout Workout { get; set; }

        public ClientEquipment Equipment { get; set; }

        public Goals Goals { get; set; }

    }
}
