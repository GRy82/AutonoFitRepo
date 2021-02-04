using AutonoFit.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.StaticClasses
{
    public static class SharedUtility
    {
        public static int repTime = 3;

        public static List<ExerciseLibrary> RandomizeExercises(List<ExerciseLibrary> exerciseLibrary)
        {
            
        }

        public static int DetermineVolume(List<int> goalIds, List<ExerciseLibrary> exerciseLibrary, FitnessDictionary fitnessMetrics, int workoutMinutes)
        {
            if (!goalIds.Contains(4) && !goalIds.Contains(5))//if cardio is involved, cut minutes in half to have half the time for cardio.
            {
                workoutMinutes /= 2;
            }

            double singleExerciseTime = (fitnessMetrics.reps * repTime + fitnessMetrics.rest) * fitnessMetrics.sets;
            int exerciseQuantity = (int)(workoutMinutes / singleExerciseTime);

            return exerciseQuantity;
        }

        public static List<ExerciseLibrary> RepackageResults(List<ExerciseLibrary> exerciseLibrary, ExerciseLibrary singleExerciseLibrary)
        {
            for (int i = 0; i < singleExerciseLibrary.results.Length; i++)
            {
                Result[] tempResult = new Result[1];
                tempResult[0] = singleExerciseLibrary.results[i];
                ExerciseLibrary tempExerciseLibrary = new ExerciseLibrary();
                tempExerciseLibrary.results = tempResult;
                exerciseLibrary.Add(tempExerciseLibrary);
            }

            return exerciseLibrary;
        }

        public static List<ExerciseLibrary> RemoveRepeats(List<ExerciseLibrary> exerciseLibrary)
        {
            List<ExerciseLibrary> revisedLibrary = new List<ExerciseLibrary> { };
            foreach (ExerciseLibrary exercise in exerciseLibrary)
            {
                if (!revisedLibrary.Contains(exercise) && exercise.results[0].id != 393)//exercise 393 is trash. It's a full workout.
                {
                    revisedLibrary.Add(exercise);
                }
            }
            return revisedLibrary;
        }

        public static int[] GetCategories(string bodySection)
        {
            int[] categories;
            switch (bodySection)
            {
                case "Upper Body":
                    categories = new int[] { 8, 10, 11, 12, 13 };
                    break;
                case "Lower Body":
                    categories = new int[] { 9, 10, 14 };
                    break;
                default:
                    categories = new int[] { 8, 9, 10, 11, 12, 13, 14 };
                    break;
            }
            return categories;
        }

        public static int[] GetMuscles(string bodySection)
        {
            int[] muscles;
            switch (bodySection)
            {
                case "Upper Body":
                    muscles = new int[] { 1, 2, 3, 4, 5, 6, 9, 12, 13, 14 };
                    break;
                case "Lower Body":
                    muscles = new int[] { 6, 7, 8, 10, 11, 14, 15 };
                    break;
                default:
                    muscles = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
                    break;
            }
            return muscles;
        }

        public static List<int> CountGoals(List<int> goalIds)
        {
            List<int> revisedGoals = new List<int> { };
            foreach (int goalId in goalIds)
            {
                if (goalId != 0)
                {
                    revisedGoals.Add(goalId);
                }
            }
            return revisedGoals;
        }
    }
}
