﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.Classes
{
    public class CardiovascularEndurance : TrainingStimulus
    {
        public float distance;
        public float duration;
        public float milePace;
        public CardiovascularEndurance()//These mimic muscular endurance to also promote endurance in other muscle groups/fiber types.
        {
            this.minReps = 11;
            this.maxReps = 20;
            this.repsInterval = 3;
            this.minRestSeconds = 30;
            this.maxRestSeconds = 45;
            this.restInterval = 15;
            this.sets = 3;
        }
    }
}
