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

        public static FitnessDictionary CalculateSetsRepsRest(List<int> goalIds, int sessionDuration, int mileMinutes, int mileSeconds)
        {

            List<TrainingStimulus> trainingStimuli = SharedUtility.DefineTrainingStimuli(goalIds);
            FitnessDictionary fitnessMetrics = SharedUtility.DefineDict(trainingStimuli);
            if (SharedUtility.CheckCardio(goalIds))
            {
                double milePace = mileMinutes + ((double)mileSeconds / 60);
                fitnessMetrics = CalculateCardio(fitnessMetrics, milePace, sessionDuration);
                fitnessMetrics.cardio = true;
            }
            else
            {
                fitnessMetrics.cardio = false;
            }
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
    }
}
