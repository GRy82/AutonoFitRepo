using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutonoFit.ViewModels;

namespace AutonoFit.StaticClasses
{
    public class CardioComponent
    {
        public SingleWorkoutVM workoutVM;
        public bool cardio;
        public string runType;
        public double milePace;
        public string paceString;
        public double distanceMiles;
        public double runDuration;
        public string durationString;

        public CardioComponent(SingleWorkoutVM workoutVM)
        {
            this.workoutVM = workoutVM;
        }

        public void SetCardioParameters()
        {
            if (SharedUtility.CheckCardio(workoutVM.GoalIds))
            {
                milePace = workoutVM.MileMinutes + ((double)workoutVM.MileSeconds / 60);
                SetCardioParameters(this, milePace, workoutVM.Minutes);
                cardio = true;
            }
            else
            {
                cardio = false;
            }
        }

        private void SetCardioParameters(CardioComponent cardioComponent, double milePace, int sessionDuration)
        {
            int runDuration = sessionDuration / 2;

            if (runDuration > 30)
                milePace *= SharedUtility.GetPaceCoefficient("Easy");
            else
                milePace *= SharedUtility.GetPaceCoefficient("Moderate");

            cardioComponent.runDuration = runDuration;
            cardioComponent.milePace = milePace;
            cardioComponent.distanceMiles = runDuration / milePace;
        }
    }
}
