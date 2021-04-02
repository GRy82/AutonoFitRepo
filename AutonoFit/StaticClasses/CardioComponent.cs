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

        public CardioComponent()
        {
        }

        public void SetCardioParameters()
        {
            if (SharedUtility.CheckCardio(workoutVM.GoalIds))
           
            runDuration = workoutVM.Minutes / 2;
            milePace = workoutVM.MileMinutes + ((double)workoutVM.MileSeconds / 60);

            if (runDuration > 30)
                milePace *= SharedUtility.GetPaceCoefficient("Easy");
            else
                milePace *= SharedUtility.GetPaceCoefficient("Moderate");

            distanceMiles = runDuration / milePace;
            durationString = SharedUtility.ConvertToMinSec((int)(runDuration * 60));
            paceString = SharedUtility.ConvertToMinSec((int)(milePace * 60));
        }
    }
}
