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
        private ExerciseLibraryService _exerciseLibraryService;

        public SingleModule(ExerciseLibraryService exerciseLibraryService)
        {
            _exerciseLibraryService = exerciseLibraryService;
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
