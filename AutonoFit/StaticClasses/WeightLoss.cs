using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.StaticClasses
{
    public class WeightLoss : TrainingStimulus
    {
        public float distance;
        public float duration;
        public float milePace;

        public WeightLoss()//these mimic hypertrophy to increase muscle mass/use to increase metabolism
        {
            this.minReps = 7;
            this.maxReps = 10;
            this.minRestSeconds = 45;
            this.maxRestSeconds = 90;
            this.restInterval = 15;
            this.sets = 5;
        }
    }
}
