using AutonoFit.ViewModels;
using AutonoFit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.StaticClasses
{
    public class FitnessParameters
    {
        public CardioComponent cardioComponent;
        public LiftingComponent liftingComponent;


        public void SetFitnessParameters(SingleWorkoutVM workoutVM)
        {
            List<TrainingStimulus> trainingStimuli = SharedUtility.SetTrainingStimuli(workoutVM.GoalIds);
            var liftingComponent = new LiftingComponent(trainingStimuli);
            liftingComponent.SetLiftParameters();

            if (SharedUtility.CheckCardio(workoutVM.GoalIds))
            {
                cardioComponent = new CardioComponent(workoutVM);
                cardioComponent.SetCardioParameters();
            }
            
        }
    }


}
