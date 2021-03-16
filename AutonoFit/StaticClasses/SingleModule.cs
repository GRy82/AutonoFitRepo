using AutonoFit.Classes;
using AutonoFit.Contracts;
using AutonoFit.Services;
using AutonoFit.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.StaticClasses
{
    public class SingleModule
    {
        private readonly IRepositoryWrapper _repo;
        private ExerciseLibraryService _exerciseLibraryService;

        public SingleModule(IRepositoryWrapper repo, ExerciseLibraryService exerciseLibraryService)
        {
            _repo = repo;
            _exerciseLibraryService = exerciseLibraryService;
        }

        public FitnessDictionary CalculateSetsRepsRest(List<int> goalIds, int sessionDuration, int mileMinutes, int mileSeconds)
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


        public FitnessDictionary CalculateCardio(FitnessDictionary cardioMetrics, double milePace, int sessionDuration)
        {
            int runDuration = sessionDuration / 2;

            if (runDuration > 30)
                milePace *= SharedUtility.GetPaceCoefficient("Easy");
            else
                milePace *= SharedUtility.GetPaceCoefficient("Moderate");

            cardioMetrics.runDuration = runDuration;
            cardioMetrics.milePace = milePace;
            cardioMetrics.distanceMiles = runDuration / milePace;


            return cardioMetrics;
        }

        public async Task<List<Result>> FindExercisesByCategory(SingleWorkoutVM workoutVM, List<Result> exerciseResults)
        {
            ExerciseLibrary singleExerciseLibrary;
            int[] categories = SharedUtility.GetCategories(workoutVM.BodySection);
            for (int i = 0; i < categories.Length; i++)
            {
                string urlCategoryString = SharedUtility.BuildEquipmentUrlString(workoutVM.Equipment) + "&category=" + categories[i];
                singleExerciseLibrary = await _exerciseLibraryService.GetExercises(urlCategoryString);
                exerciseResults = SharedUtility.RepackageResults(exerciseResults, singleExerciseLibrary);
            }

            return exerciseResults;
        }

        public async Task<List<Result>> FindExercisesByMuscles(SingleWorkoutVM workoutVM, List<Result> exerciseResults)
        {
            ExerciseLibrary singleExerciseLibrary;
            int[] muscles = SharedUtility.GetMuscles(workoutVM.BodySection);
            string urlMusclesString = null;
            for (int j = 0; j < muscles.Length; j++)
            {
                urlMusclesString += "&muscles=" + muscles[j];
            }
            urlMusclesString = SharedUtility.BuildEquipmentUrlString(workoutVM.Equipment) + urlMusclesString;
            singleExerciseLibrary = await _exerciseLibraryService.GetExercises(urlMusclesString);
            exerciseResults = SharedUtility.RepackageResults(exerciseResults, singleExerciseLibrary);

            return exerciseResults;
        }
    }
}
