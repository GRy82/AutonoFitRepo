using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.StaticClasses
{
    public class Strength: TrainingStimulus
    {
        public Strength()
        {
            this.minReps = 3;
            this.maxReps = 6;
            this.minRestSeconds = 150;
            this.maxRestSeconds = 210;
            this.sets = 4;
        }
    }
}
