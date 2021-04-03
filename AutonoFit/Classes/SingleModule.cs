using AutonoFit.Classes;
using AutonoFit.Contracts;
using AutonoFit.Services;
using AutonoFit.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.Classes
{
    public class SingleModule
    {
        private ExerciseLibraryService _exerciseLibraryService;

        public SingleModule(ExerciseLibraryService exerciseLibraryService)
        {
            _exerciseLibraryService = exerciseLibraryService;
        }


        public async Task<List<Exercise>> FindExercisesByCategory(SingleWorkoutVM workoutVM, List<Exercise> exercises)
        {
            ExerciseLibrary exerciseLibrary;
            int[] categories = SharedUtility.GetCategories(workoutVM.BodySection);
            for (int i = 0; i < categories.Length; i++)
            {
                string urlCategoryString = SharedUtility.BuildEquipmentUrlString(workoutVM.Equipment) + "&category=" + categories[i];
                exerciseLibrary = await _exerciseLibraryService.GetExercises(urlCategoryString);
                exercises = SharedUtility.AddLibrarytoExercises(exercises, exerciseLibrary);
            }

            return exercises;
        }

        public async Task<List<Exercise>> FindExercisesByMuscles(SingleWorkoutVM workoutVM, List<Exercise> exercises)
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
            exercises = SharedUtility.AddLibrarytoExercises(exercises, singleExerciseLibrary);

            return exercises;
        }
    }
}
