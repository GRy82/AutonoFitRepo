using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.Classes
{
    public class Strength: TrainingStimulus
    {
        public Strength()
        {
            this.minReps = 3;
            this.maxReps = 6;
            this.repsInterval = 1;
            this.minRestSeconds = 150;
            this.maxRestSeconds = 210;
            this.restInterval = 20;
            this.sets = 4;
        }
    }
}
