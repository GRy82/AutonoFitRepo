using AutonoFit.Models;
using AutonoFit.StaticClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.ViewModels
{
    public class ProgramSetupVM
    {

        public List<Goals> AvailableGoals { get; set; }

        public List<int> GoalIds { get; set; }


        public int Minutes { get; set; }

        public int MileMinutes { get; set; }

        public int MileSeconds { get; set; }

        public string ErrorMessage { get; set; }
    }
}
