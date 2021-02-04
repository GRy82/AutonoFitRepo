using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.StaticClasses
{
    public abstract class TrainingStimulus
    {
        public int minReps;
        public int maxReps;
        public int minRestSeconds;
        public int maxRestSeconds;
        public int sets;
    }
}
