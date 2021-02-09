using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.StaticClasses
{
    public class Hypertrophy : TrainingStimulus
    {
        public Hypertrophy()
        {
            this.minReps = 7;
            this.maxReps = 10;
            this.minRestSeconds = 45;
            this.restInterval = 15;
            this.maxRestSeconds = 90;
            this.sets = 5;
        }
    }
}
