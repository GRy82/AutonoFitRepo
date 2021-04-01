using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.StaticClasses
{
    public class LiftingComponent
    {
        List<TrainingStimulus> trainingStimuli;
        public int reps;
        public int rest;
        public string restString;
        public int sets;

        public LiftingComponent(List<TrainingStimulus> trainingStimuli)
        {
            this.trainingStimuli = trainingStimuli;
        }

        public void SetLiftParameters()
        {
            int repsSum = 0;
            int restSum = 0;
            int setsSum = 0;
            foreach (TrainingStimulus stimuli in trainingStimuli)
            {
                int middleGroundReps = (stimuli.maxReps + stimuli.minReps) / 2;
                repsSum += middleGroundReps;

                int middleGroundRest = (stimuli.maxRestSeconds + stimuli.minRestSeconds) / 2;
                restSum += middleGroundRest;

                setsSum += stimuli.sets;
            }

            reps = (int)(repsSum / trainingStimuli.Count);
            rest = (int)restSum / trainingStimuli.Count;
            sets = (int)setsSum / trainingStimuli.Count;
            restString = SharedUtility.ConvertToMinSecString(rest);
        }
    }
}
