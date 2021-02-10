using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.StaticClasses
{
    public class MuscularEndurance : TrainingStimulus
    {
        public int repInterval;
        public MuscularEndurance()
        {
            this.minReps = 11;
            this.maxReps = 20;
            this.repInterval = 3;
            this.minRestSeconds = 30;
            this.maxRestSeconds = 45;
            this.restInterval = 15;
            this.sets = 5;
        }
    }
}
