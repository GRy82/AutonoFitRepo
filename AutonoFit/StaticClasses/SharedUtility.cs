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
        public const int repTime = 4;


        public static string BuildEquipmentUrlString(List<ClientEquipment> equipmentList)
        {
            string urlString = "https://wger.de/api/v2/exercise?language=2&limit=100&equipment=7";
            foreach (ClientEquipment piece in equipmentList)
            {
                urlString += "&equipment=" + piece.EquipmentId;
            }

            return urlString;
        }

        public static List<ClientExercise> CopyAsClientExercises(List<Exercise> randomlyChosenExercises, SingleWorkoutVM workoutVM, FitnessParameters fitnessParamenters)
        {
            List<ClientExercise> workoutExercises = new List<ClientExercise> { };
            LiftingComponent lift = fitnessParamenters.liftingComponent;
            foreach (Exercise exercise in randomlyChosenExercises)
            {
                ClientExercise clientExercise = new ClientExercise();
                clientExercise.ClientId = workoutVM.Client.ClientId;
                clientExercise.ExerciseId = exercise.id;
                clientExercise.Reps = lift.reps;
                clientExercise.RestSeconds = lift.rest;
                clientExercise.Sets = lift.sets;
                
                workoutExercises.Add(clientExercise);
            }

            return workoutExercises;
        }

        public static ClientExercise CopyAsClientExercises(Exercise randomlyChosenExercise, int clientId, FitnessParameters fitnessMetrics)
        {
            ClientExercise exercise = new ClientExercise();
            exercise.ClientId = clientId;
            exercise.ExerciseId = randomlyChosenExercise.id;
            exercise.Reps = fitnessMetrics.reps;
            exercise.RestSeconds = fitnessMetrics.rest;
            exercise.Sets = fitnessMetrics.sets;

            return exercise;
        }

        public static List<Exercise> RandomizeExercises(List<Exercise> exerciseResults, int exerciseQuantity)
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

        public static int GetExerciseQty(FitnessParameters fitnessParameters, int workoutMinutes)
        {
            double singleExerciseTime = GetSingleExerciseTime(fitnessParameters);
            return (int)((workoutMinutes * 60) / singleExerciseTime);//convert workout minutes to seconds, then divide by seconds per exerise.
        }

        public static double GetSingleExerciseTime(FitnessParameters fitnessParameters)// return value is in seconds
        {
            return (fitnessParameters.liftingComponent.reps * repTime + fitnessParameters.liftingComponent.rest)
                    * fitnessParameters.liftingComponent.sets; 

        }

        public static List<Exercise> AddLibrarytoExercises(List<Exercise> exercises, ExerciseLibrary exerciseLibrary)
        {
            for (int i = 0; i < exerciseLibrary.results.Length; i++)
            {
                Exercise tempResult = exerciseLibrary.results[i];
                exercises.Add(tempResult);
            }

            return exercises;
        }

        public static List<Exercise> RemoveRepeats(List<Exercise> exercises)
        {
            List<Exercise> revisedResults = new List<Exercise> { };
            List<int> addedExerciseIds = new List<int> { };
            foreach (Exercise exercise in exercises)
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

        public static Exercise SelectExercise(string bodyParts, List<Exercise> exercises, List<Exercise> todaysExercises)
        {
            List<Result> possibleExercises = new List<Result> { };
            int[] muscles = GetMuscles(bodyParts);
            int[] categories = GetCategories(bodyParts);

            foreach (Result exercise in exercises)
            {
                for (int i = 0; i < muscles.Length; i++)
                {
                    if ((exercise.muscles.Contains(muscles[i]) || exercise.muscles_secondary.Contains(muscles[i]) || (i < categories.Length && exercise.category == categories[i])) && exercise.id != 393)//exercise 393 is a full workout
                    {
                        possibleExercises.Add(exercise);
                    }
                }
            }

            List<Result> singleExercise;
            do
            {
                singleExercise = new List<Result> { };
                singleExercise = RandomizeExercises(possibleExercises, 1);
            } while (todaysExercises.Contains(singleExercise[0]));
            return singleExercise[0];
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

        public static string ConvertToMinSecString(int input)//input in seconds. This rounds to nearest 15 seconds. 
            //Get rid of this method if it is no longer needed with the addition of ConvertToMinSec method.
        {
            int remainder = input % 15;
            int newSeconds = remainder < 8 ? input - remainder : input - remainder + 15;
            int minutes = newSeconds / 60;
            int seconds = newSeconds % 60;
            string newString = null;
            if (minutes != 0)
            {
                newString += minutes + " min ";
            }
            if (seconds != 0)
            {
                newString += seconds + " sec";
            }
            return newString;
        }

        public static string ConvertToMinSec(int input) //inputin seconds. This does not round to nearest 15 seconds.
        {
            int minutes = input / 60;
            int seconds = input % 60;
            string newString = null;
            if (minutes != 0)
            {
                newString += minutes + " min ";
            }
            if (seconds != 0)
            {
                newString += seconds + " sec";
            }
            return newString;
        }

        public static FitnessParameters ConvertFitnessDictCardioValues(FitnessParameters fitDict)
        {
            fitDict.durationString = ConvertToMinSec((int)(fitDict.runDuration * 60));
            fitDict.paceString = ConvertToMinSec((int)(fitDict.milePace * 60));
            return fitDict;
        }

        public static string RemoveTags(string stringWithTags)
        {
            string[] tagsArray = new string[] { "<p>", "<strong>", "<ol>", "<li>", "<ul>" };
            for(int i = 0; i < tagsArray.Length; i++)
            {
                stringWithTags = stringWithTags.Replace(tagsArray[i], "");
                string tempstring = "<" + tagsArray[i].Replace('<', '/');
                stringWithTags = stringWithTags.Replace(tempstring, "");
            }

            return stringWithTags;
        }

        public static bool CheckCardio(List<int> goalIds)
        {
            if (goalIds.Contains(4) || goalIds.Contains(5))
            {
                return true;
            }
            return false;
        }

        public static double GetPaceCoefficient(string runType)
        {
            double paceCoefficient = 0;
            switch (runType)
            {
                case "Easy":
                    paceCoefficient = 1.5;
                    break;
                case "Moderate":
                    paceCoefficient = 1.43;
                    break;
                case "Long":
                    paceCoefficient = 1.39;
                    break;
                case "Speed":
                    paceCoefficient = 1.1;
                    break;
            }

            return paceCoefficient;
        }

        public static List<TrainingStimulus> SetTrainingStimuli(List<int> goalIds)
        {
            List<TrainingStimulus> trainingStimuli = new List<TrainingStimulus> { };
            foreach (int goalId in goalIds)
            {
                switch (goalId)
                {
                    case 1:
                        trainingStimuli.Add(new Strength());
                        break;
                    case 2:
                        trainingStimuli.Add(new Hypertrophy());
                        break;
                    case 3:
                        trainingStimuli.Add(new MuscularEndurance());
                        break;
                    case 4:
                        trainingStimuli.Add(new CardiovascularEndurance());
                        break;
                    case 5:
                        trainingStimuli.Add(new WeightLoss());
                        break;
                }
            }
            return trainingStimuli;
        }

        
    }
}
