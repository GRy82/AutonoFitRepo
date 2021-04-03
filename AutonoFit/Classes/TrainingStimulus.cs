using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.Classes
{
    public abstract class TrainingStimulus
    {
        public int minReps;
        public int maxReps;
        public int repsInterval;
        public int minRestSeconds;
        public int maxRestSeconds;
        public int restInterval;
        public int sets;
    }
}
