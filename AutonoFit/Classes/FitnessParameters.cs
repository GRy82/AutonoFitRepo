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

        public async Task<FitnessParameters> GetTodaysCardio(FitnessParameters fitnessParameters, List<ClientWorkout> recentWorkoutCycle, int todaysGoalNumber, ClientProgram currentProgram)
        {
            string runType = null;
            if (currentProgram.GoalCount == 1 && currentProgram.DaysPerWeek == 6)
            {
                var recentCardioWorkouts = GetRecentCardioOnly(recentWorkoutCycle);
            }

            if (currentProgram.GoalCount == 1 && recentWorkoutCycle.Count != 0)//One goal, there is a recent cardio
            {
                runType = AdvanceRunType(recentWorkoutCycle[0].RunType);
            }
            else if (currentProgram.GoalCount == 2 && recentWorkoutCycle.Count > 1)//2 goals, there's a recent cardio
            {
                runType = AdvanceRunType(recentWorkoutCycle[1].RunType);
            }
            else //this is the first run, or first workout
            {
                runType = "Easy";
            }
            //Corner Case
            if (runType == "Easy" && currentProgram.GoalCount == 1 && currentProgram.DaysPerWeek == 6) // == easy after the advancement.
            {
                fitnessParameters.cardioComponent.runType = "6 Lift"; //this code will indicate a lift should be done instead of a run.  
                return fitnessParameters;//returned with only one change.
            }

            fitnessParameters.cardio = true;
            fitnessParameters = await GenerateRun(currentProgram, runType, fitnessParameters, recentWorkoutCycle);
            fitnessParameters = SharedUtility.ConvertFitnessDictCardioValues(fitnessMetrics);

            return fitnessParameters;
        }

        private async Task<FitnessParameters> GenerateRun(ClientProgram currentProgram, string runType, FitnessParameters fitnessMetrics, List<ClientWorkout> recentWorkoutCycle)
        {
            double paceCoefficient = SharedUtility.GetPaceCoefficient(runType);
            if ((currentProgram.GoalCount == 1 && recentWorkoutCycle.Count > 1) || (currentProgram.GoalCount == 2 && recentWorkoutCycle.Count > 3))
            {
                await CheckCardioProgression(currentProgram, recentWorkoutCycle);
            }

            fitnessMetrics.milePace = (double)currentProgram.MileMinutes + (Convert.ToDouble(currentProgram.MileSeconds) / 60);
            fitnessMetrics.milePace *= paceCoefficient;
            fitnessMetrics.runDuration = getRunDuration(currentProgram.MinutesPerSession, runType);
            fitnessMetrics.distanceMiles = fitnessMetrics.runDuration / fitnessMetrics.milePace;
            fitnessMetrics.runType = runType;
            if (runType == "Easy")//all easy runs will need time to be accompanied by an aerobic lifting workout.
            {
                fitnessMetrics.runDuration /= 2;
            }

            return fitnessMetrics;
        }

        public async Task CheckCardioProgression(ClientProgram currentProgram, List<ClientWorkout> recentWorkoutCycle)
        {
            bool readyToProgress = (currentProgram.GoalCount == 1 && recentWorkoutCycle[0].CardioRPE < recentWorkoutCycle[1].CardioRPE) ||
                (currentProgram.GoalCount == 2 && recentWorkoutCycle[1].CardioRPE < recentWorkoutCycle[3].CardioRPE);

            if (readyToProgress)
            {
                if (currentProgram.MileSeconds == 0)
                {
                    currentProgram.MileMinutes -= 1;
                    currentProgram.MileSeconds = 59;
                }
                else
                {
                    currentProgram.MileSeconds -= 1;
                }
                _repo.ClientProgram.EditClientProgram(currentProgram);
                await _repo.SaveAsync();
            }
        }

        public string AdvanceRunType(string runType)
        {
            if (runType == "6 Lift")//This is a corner case. See calling method for reasoning.
            {
                return "Easy";
            }
            string[] runTypes = new string[] { "Easy", "Moderate", "Long", "Speed" };
            int index = runType.IndexOf(runType);
            string newRunType = index != (runTypes.Length - 1) ? runTypes[index + 1] : runTypes[0];

            return newRunType;
        }

        public int getRunDuration(int sessionMinutes, string runType)
        {
            int runDuration = 0;
            int halfSessionMinutes = sessionMinutes / 2;

            switch (runType)
            {
                case "Easy":
                    runDuration = Math.Min(30, halfSessionMinutes);
                    break;
                case "Moderate":
                    runDuration = Math.Min(sessionMinutes, 45);
                    break;
                case "Long":
                    runDuration = sessionMinutes;
                    break;
                case "Speed":
                    runDuration = Math.Min(15, halfSessionMinutes);
                    break;
            }

            return runDuration;
        }

        public List<ClientWorkout> GetRecentCardioOnly(List<ClientWorkout> recentWorkoutCycle)//Makes sure that no "6 Lift" exercises make it in the collection.
        {
            List<ClientWorkout> recentCardioWorkouts = new List<ClientWorkout> { };
            foreach (ClientWorkout workout in recentWorkoutCycle)
            {
                if (workout.GoalId < 4 || (workout.GoalId > 3 && workout.RunType != null))
                {
                    recentCardioWorkouts.Add(workout);
                }
            }
            return recentCardioWorkouts;
        }
    }


}
