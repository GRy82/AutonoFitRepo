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

        public static Dictionary<string, int> CalculateSetsRepsRest(List<int> goalIds)
        {
            Dictionary<string, int> setsRepsRest = new Dictionary<string, int> { };
            List<TrainingStimulus> trainingStimuli = DefineTrainingStimuli(goalIds);
            setsRepsRest["reps"] = DefineReps(trainingStimuli);
            return setsRepsRest;
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
                        break;
                    case 5:
                        break;
                }
            }
            return trainingStimuli;
        }

        private static int RepsOneGoal(List<TrainingStimulus> trainingStimuli)
        {
            int sum;

            return 3;
        }
    }
}
