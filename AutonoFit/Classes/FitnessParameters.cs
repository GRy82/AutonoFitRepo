using AutonoFit.ViewModels;
using AutonoFit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.Classes
{
    public class FitnessParameters
    {
        public CardioComponent cardioComponent;
        public LiftingComponent liftingComponent;

        public FitnessParameters(CardioComponent cardioComponent = null, LiftingComponent liftingComponent = null)
        {

        }

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

        public void SetFitnessParameters(int todaysGoalNumber)
        {
            if (CardioIsNeeded(todaysGoalNumber))
                cardioComponent = new CardioComponent();

        }

        private bool CardioIsNeeded(int todaysGoalNumber)
        {
            return (todaysGoalNumber == 4 || todaysGoalNumber == 5);
        }
     
    }


}
