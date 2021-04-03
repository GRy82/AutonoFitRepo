using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutonoFit.Contracts;
using AutonoFit.Models;

namespace AutonoFit.Classes
{
    public class Prescription
    {
        private IRepositoryWrapper _repo;

        public Prescription(IRepositoryWrapper repo)
        {
            _repo = repo;
        }

        public async Task<FitnessParameters> GetTodaysCardio(FitnessParameters fitnessParameters, List<ClientWorkout> recentWorkoutCycle, int todaysGoalNumber, ClientProgram currentProgram)
        {
            string runType = null;
            if (currentProgram.GoalCount == 1 && currentProgram.DaysPerWeek == 6)
            {
                var recentCardioWorkouts = GetRecentCardioOnly(recentWorkoutCycle);//Excludes workouts marked w/ cardio goals that were a lift instead, due to excess cardio that week.
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
            CardioComponent cardioc = await GetCardioComponent(currentProgram, runType, recentWorkoutCycle);
            fitnessParameters = ConvertFitnessDictCardioValues(fitnessParameters);

            return fitnessParameters;
        }

        public async Task<CardioComponent> GetCardioComponent(ClientProgram currentProgram, string runType, List<ClientWorkout> recentWorkoutCycle)
        {
            CardioComponent cardioComponent = CardioComponentFactoryMethod(runType);
            if ((currentProgram.GoalCount == 1 && recentWorkoutCycle.Count > 1) || (currentProgram.GoalCount == 2 && recentWorkoutCycle.Count > 3))
            {
                await CheckCardioProgression(currentProgram, recentWorkoutCycle);
            }

            cardioComponent.milePace = (double)currentProgram.MileMinutes + (Convert.ToDouble(currentProgram.MileSeconds) / 60);
            cardioComponent.milePace *= cardioComponent.paceCoefficient;
            cardioComponent.runDuration = getRunDuration(currentProgram.MinutesPerSession, runType);
            cardioComponent.distanceMiles = cardioComponent.runDuration / cardioComponent.milePace;
            cardioComponent.runType = runType;
            if (runType == "Easy")//all easy runs will need time to be accompanied by an aerobic lifting workout.
            {
                cardioComponent.runDuration /= 2;
            }

            return cardioComponent;
        }

        public CardioComponent CardioComponentFactoryMethod(string runType) 
        {
            CardioComponent cardioComponent = null;
            switch (runType)
            {
                case "Easy":
                    cardioComponent = new EasyRun();
                    break;
                case "Moderate":
                    cardioComponent = new ModerateRun();
                    break;
                case "Long":
                    cardioComponent = new LongRun();
                    break;
                case "Speed":
                    cardioComponent = new SpeedRun();
                    break;
            }

            return cardioComponent;
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
