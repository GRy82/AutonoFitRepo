using AutonoFit.Models;
using AutonoFit.Services;
using AutonoFit.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.StaticClasses
{
    public static class SharedUtility
    {
        public static int repTime = 3;

        public static List<ClientExercise> CopyAsClientExercises(List<Result> randomlyChosenExercises, SingleWorkoutVM workoutVM, FitnessDictionary fitnessMetrics)
        {
            List<ClientExercise> workoutExercises = new List<ClientExercise> { };
            foreach (Result result in randomlyChosenExercises)
            {
                ClientExercise exercise = new ClientExercise();
                exercise.ClientId = workoutVM.Client.ClientId;
                exercise.ExerciseId = result.id;
                exercise.Reps = fitnessMetrics.reps;
                exercise.RestSeconds = fitnessMetrics.rest;
                
                workoutExercises.Add(exercise);
            }

            return workoutExercises;
        }

        public static List<Result> RandomizeExercises(List<Result> exerciseResults, int exerciseQuantity)
        {
            List<Result> selectedExercises = new List<Result> { };
            Random rand = new Random();
            int index;
            while (selectedExercises.Count < exerciseQuantity)
            {
                do
                {
                    index = rand.Next(0, exerciseResults.Count);
                } while (selectedExercises.Contains(exerciseResults.ElementAt(index)));

                selectedExercises.Add(exerciseResults.ElementAt(index));
            }

            return selectedExercises;
        }

        public static int DetermineVolume(List<int> goalIds, FitnessDictionary fitnessMetrics, int workoutMinutes)
        {
            if (goalIds.Contains(4) || goalIds.Contains(5))//if cardio is involved, cut minutes in half to have half the time for cardio.
            {
                workoutMinutes /= 2;
            }

            double singleExerciseTime = (fitnessMetrics.reps * repTime + fitnessMetrics.rest) * fitnessMetrics.sets;
            int exerciseQuantity = (int)((workoutMinutes * 60) / singleExerciseTime);//convert workout minutes to seconds, then divide by seconds per exerise.

            return exerciseQuantity;
        }

        public static List<Result> RepackageResults(List<Result> exerciseResults, ExerciseLibrary singleExerciseLibrary)
        {
            for (int i = 0; i < singleExerciseLibrary.results.Length; i++)
            {
                Result[] tempResult = new Result[1];
                tempResult[0] = singleExerciseLibrary.results[i];
                exerciseResults.Add(tempResult[0]);
            }

            return exerciseResults;
        }

        public static List<Result> RemoveRepeats(List<Result> exerciseResults)
        {
            List<Result> revisedResults = new List<Result> { };
            List<int> addedExerciseIds = new List<int> { };
            foreach (Result exercise in exerciseResults)
            {
                if (!addedExerciseIds.Contains(exercise.id) && exercise.id != 393)//exercise 393 is trash. It's a full workout.
                {
                    revisedResults.Add(exercise);
                    addedExerciseIds.Add(exercise.id);
                }
            }
            return revisedResults;
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

        public static string ConvertToMinSecString(int input)//input in seconds
        {
            int remainder = input % 15;
            int newSeconds = remainder < 8 ? input - remainder : input - remainder + 15;
            int minutes = newSeconds / 60;
            int seconds = newSeconds % 60;
            string newString = null;
            if (minutes != 0)
            {
                newString += minutes + "mins ";
            }
            if (seconds != 0)
            {
                newString += seconds + "secs";
            }
            return newString;
        }

        public static FitnessDictionary ConvertFitnessDictCardioValues(FitnessDictionary fitDict)
        {
            fitDict.durationString = ConvertToMinSecString((int)(fitDict.runDuration * 60));
            fitDict.paceString = ConvertToMinSecString((int)(fitDict.milePace * 60));
            return fitDict;
        }
    }
}
