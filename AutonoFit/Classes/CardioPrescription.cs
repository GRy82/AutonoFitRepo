using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutonoFit.Contracts;
using AutonoFit.Models;
using AutonoFit.Services;

namespace AutonoFit.Classes
{
    public class CardioPrescription
    {
        private IRepositoryWrapper _repo;
        private ExerciseLibraryService _exerciseLibraryService;

        public CardioPrescription(IRepositoryWrapper repo, ExerciseLibraryService exerciseLibraryService)
        {
            _repo = repo;
            _exerciseLibraryService = exerciseLibraryService;
        }

        public async Task<CardioComponent> GetTodaysCardio(List<ClientWorkout> recentWorkoutCycle, ClientProgram currentProgram)
        {
            string runType = null;
            if (currentProgram.GoalCount == 1 && currentProgram.DaysPerWeek == 6)
            {
                recentWorkoutCycle = GetRecentCardioOnly(recentWorkoutCycle);//Excludes workouts marked w/ cardio goals that were a lift instead, due to excess cardio that week.
            }

            if (currentProgram.GoalCount == 1 && recentWorkoutCycle.Count != 0)//One goal, there is a recent cardio
            {
                runType = AdvanceRunType(recentWorkoutCycle[0].RunType);
            }
            else if (currentProgram.GoalCount == 2 && recentWorkoutCycle.Count > 1)//2 goals, there's a recent cardio 2 workouts ago
            {
                runType = AdvanceRunType(recentWorkoutCycle[1].RunType);
            }
            else //this is the first run, or first workout
            {
                runType = "Easy";
            }
            //Edge Case
            if (runType == "Easy" && currentProgram.GoalCount == 1 && currentProgram.DaysPerWeek == 6) // == easy after the advancement.
            {
                return new SixLift(); //this object will indicate a lift should be done instead of a run. This is because of excessive cardio in the week.
            }

            //fitnessParameters.cardio = true;
            return await CreateCardioComponent(currentProgram, runType, recentWorkoutCycle);
            //fitnessParameters = ConvertFitnessDictCardioValues(fitnessParameters);//not sure what this does
        }

        public async Task<CardioComponent> CreateCardioComponent(ClientProgram currentProgram, string runType, List<ClientWorkout> recentWorkoutCycle)
        {
            CardioComponent cardioComponent = CardioComponentFactoryMethod(runType);
            //increases difficulty of cardio if 2 consecutive workouts have been done with decreasing RPE scores.
            if ((currentProgram.GoalCount == 1 && recentWorkoutCycle.Count > 1) || (currentProgram.GoalCount == 2 && recentWorkoutCycle.Count > 3))
                await CheckCardioProgression(currentProgram, recentWorkoutCycle);

            cardioComponent.milePace = (double)currentProgram.MileMinutes + (Convert.ToDouble(currentProgram.MileSeconds) / 60);
            cardioComponent.milePace *= cardioComponent.paceCoefficient;
            cardioComponent.runDuration = cardioComponent.GetRunDuration(currentProgram.MinutesPerSession);
            cardioComponent.distanceMiles = cardioComponent.runDuration / cardioComponent.milePace;
            cardioComponent.runType = runType;

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
                case "6-lift":
                    cardioComponent = new SixLift();
                    break;
            }

            return cardioComponent;
        }

        //Accounts for multiple goals represented in recent workouts.
        public async Task CheckCardioProgression(ClientProgram currentProgram, List<ClientWorkout> recentWorkoutCycle)
        { // Decrease base mile pace if RPE difficulty at given base pace is decreasing. This is regardless of run type.
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
