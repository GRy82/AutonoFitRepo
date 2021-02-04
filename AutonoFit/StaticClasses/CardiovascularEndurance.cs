using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.StaticClasses
{
    public class CardiovascularEndurance : TrainingStimulus
    {
        public float distance;
        public float duration;
        public float milePace;
        public CardiovascularEndurance()
        {
            this.minReps = 12;
            this.maxReps = 20;
            this.minRestSeconds = 30;
            this.maxRestSeconds = 45;
            this.sets = 5;
        }
    }
}
