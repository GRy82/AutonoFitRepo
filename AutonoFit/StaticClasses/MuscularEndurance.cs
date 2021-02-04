using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.StaticClasses
{
    public class MuscularEndurance : TrainingStimulus
    {
        public MuscularEndurance()
        {
            this.minReps = 12;
            this.maxReps = 20;
            this.minRestSeconds = 30;
            this.maxRestSeconds = 45;
            this.sets = 5;
        }
    }
}
