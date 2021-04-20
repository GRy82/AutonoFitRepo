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
            string runType;
            if (currentProgram.GoalCount == 1 && currentProgram.DaysPerWeek == 6)
                recentWorkoutCycle = OmitSupplementaryLifts(recentWorkoutCycle);//Excludes workouts marked w/ cardio goals that were a lift instead, due to excess cardio that week.

            if (recentWorkoutCycle.Count == 0) //this is the first run. Start on easy
                runType = "Easy";
            else
                runType = AdvanceRunType(recentWorkoutCycle[0].RunType);// not the first run. Cycle run type.

            if(runType == "6-lift")
            {
                if (currentProgram.GoalCount == 1 && currentProgram.DaysPerWeek > 5)//excessive cardio, trigger a lift instead.
                    return new SixLift();

                runType = AdvanceRunType("6-lift");//Cardio not actually excessive. Toggle one more time to a legitimate run type.
            }

            return await CreateCardioComponent(currentProgram, runType, recentWorkoutCycle);
            //fitnessParameters = ConvertFitnessDictCardioValues(fitnessParameters);//not sure what this does
        }

        public async Task<CardioComponent> CreateCardioComponent(ClientProgram currentProgram, string runType, List<ClientWorkout> recentWorkoutCycle)
        {
            CardioComponent cardioComponent = CardioComponentFactoryMethod(runType);
       
            //dec. pace/inc. difficulty if 2 consecutive cardio workouts have been done with decreasing RPE scores.
            if (recentWorkoutCycle.Count > 1)
                await CheckCardioProgression(currentProgram, recentWorkoutCycle);

            cardioComponent.runType = runType;
            cardioComponent.milePace = (double)currentProgram.MileMinutes + (Convert.ToDouble(currentProgram.MileSeconds) / 60);
            cardioComponent.milePace *= SharedUtility.GetPaceCoefficient(cardioComponent.runType);
            cardioComponent.runDuration = cardioComponent.GetRunDuration(currentProgram.MinutesPerSession);
            cardioComponent.distanceMiles = cardioComponent.runDuration / cardioComponent.milePace;

            return cardioComponent;
        }

        public CardioComponent CardioComponentFactoryMethod(string runType) 
        {
            CardioComponent cardioComponent = null;
            switch (runType)
            {
                case "Easy":
                    return new EasyRun();
                case "Moderate":
                    return new ModerateRun();
                 case "Long":
                    return new LongRun();
                case "Speed":
                    return new SpeedRun();
            }
            return cardioComponent;
        }

        //Accounts for multiple goals represented in recent workouts.
        public async Task CheckCardioProgression(ClientProgram currentProgram, List<ClientWorkout> recentWorkoutCycle)
        { // Decrease base mile pace if RPE difficulty at given base pace is decreasing. This is regardless of run type.
            bool readyToProgress = (recentWorkoutCycle[0].CardioRPE < recentWorkoutCycle[1].CardioRPE);

            if (readyToProgress)
            {
                if (currentProgram.MileSeconds == 0)
                {
                    currentProgram.MileMinutes -= 1;
                    currentProgram.MileSeconds = 59;
                }
                else
                    currentProgram.MileSeconds -= 1;

                _repo.ClientProgram.EditClientProgram(currentProgram);
                await _repo.SaveAsync();
            }
        }

        public string AdvanceRunType(string runType)
        {
            string[] runTypes = new string[] { "Easy", "Moderate", "Long", "Speed", "6-lift" };
            int index = runType.IndexOf(runType);
            string newRunType = index != (runTypes.Length - 1) ? runTypes[index + 1] : runTypes[0];

            return newRunType;
        }

        public List<ClientWorkout> OmitSupplementaryLifts(List<ClientWorkout> recentWorkoutCycle)//Makes sure that no "6 Lift" exercises make it in the collection.
        {
            List<ClientWorkout> recentCardioWorkouts = new List<ClientWorkout> { };

            foreach (ClientWorkout workout in recentWorkoutCycle)
                if (workout.GoalId > 3 && workout.RunType != null)//Lift goal or run goal with no run type
                    recentCardioWorkouts.Add(workout);
            
            return recentCardioWorkouts;
        }
    }
}
