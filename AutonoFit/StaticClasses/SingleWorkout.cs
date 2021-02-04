using AutonoFit.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.StaticClasses
{
    public static class SingleWorkout
    {
        private static readonly IRepositoryWrapper _repo;

        public static FitnessDictionary CalculateSetsRepsRest(List<int> goalIds, int sessionDuration, double milePace = 12)
        {
            List<TrainingStimulus> trainingStimuli = DefineTrainingStimuli(goalIds);
            FitnessDictionary fitnessMetrics = DefineDict(trainingStimuli);
            if (CheckCardio(trainingStimuli))
            {
                FitnessDictionary cardioMetrics = CalculateCardio(fitnessMetrics, milePace, sessionDuration);
            }
            return fitnessMetrics;
        }

       
        public static List<TrainingStimulus> DefineTrainingStimuli(List<int> goalIds)
        {
            List<TrainingStimulus> trainingStimuli = new List<TrainingStimulus> { };
            foreach (int goalId in goalIds)
            {
                switch (goalId)
                {
                    case 1:
                        trainingStimuli.Add(new Strength());
                        break;
                    case 2:
                        trainingStimuli.Add(new Hypertrophy());
                        break;
                    case 3:
                        trainingStimuli.Add(new MuscularEndurance());
                        break;
                    case 4:
                        trainingStimuli.Add(new CardiovascularEndurance());
                        break;
                    case 5:
                        trainingStimuli.Add(new WeightLoss());
                        break;
                }
            }
            return trainingStimuli;
        }



        private static FitnessDictionary DefineDict(List<TrainingStimulus> trainingStimuli)
        {
            FitnessDictionary fitnessMetrics = new FitnessDictionary();
            int repsSum = 0;
            int restSum = 0;
            int setsSum = 0;
            foreach(TrainingStimulus stimuli in trainingStimuli)
            {
                int middleGroundReps = (stimuli.maxReps + stimuli.minReps) / 2;
                repsSum += middleGroundReps;
                int middleGroundRest = (stimuli.maxRestSeconds + stimuli.minRestSeconds) / 2;
                restSum += middleGroundRest;
                setsSum += stimuli.sets;
            }

            fitnessMetrics.reps = (int)(repsSum / trainingStimuli.Count);
            fitnessMetrics.rest = (int)restSum / trainingStimuli.Count;
            fitnessMetrics.sets = (int)setsSum / trainingStimuli.Count;
            return fitnessMetrics;
        }

        public static FitnessDictionary CalculateCardio(FitnessDictionary cardioMetrics, double milePace, int sessionDuration)
        {
            sessionDuration /= 2;
            cardioMetrics.runDuration = sessionDuration;
            cardioMetrics.milePace = milePace;
            cardioMetrics.distanceMiles = sessionDuration / milePace;

            return cardioMetrics;
        }

        public static bool CheckCardio(List<TrainingStimulus> trainingStimuli)
        {
            foreach(TrainingStimulus stimuli in trainingStimuli)
            {
                Type type = stimuli.GetType();
                if (type.Equals(new WeightLoss().GetType()) || type.Equals(new CardiovascularEndurance().GetType())) {
                    return true;
                }
            }
            return false;
        }
    }
}
