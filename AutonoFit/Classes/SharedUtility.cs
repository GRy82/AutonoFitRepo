﻿using AutonoFit.Models;
using AutonoFit.Services;
using AutonoFit.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutonoFit.Classes
{
    public static class SharedUtility
    {
        public static int expectedUrlLengthMax = 150;
        public static int expectedCardioStringMax = 50;
        public const int repTime = 4;
        public const int muscEndTransitionTime = 45;
        public const int otherLiftTransitionTime = 10;
        public static Random rand = new Random();


        public static String BuildEquipmentUrlString(List<ClientEquipment> equipmentList)
        {
            StringBuilder url = new StringBuilder("https://wger.de/api/v2/exercise?language=2&limit=100&equipment=7,", expectedUrlLengthMax);
            foreach (ClientEquipment piece in equipmentList)
            {

                url.Append(piece.EquipmentId).Append(',');
            }
            url.Remove(url.Length - 1, 1);

            return url.ToString();
        }

        public static List<Exercise> RandomizeExercises(List<Exercise> exerciseResults, int exerciseQuantity)
        {
            List<Exercise> selectedExercises = new List<Exercise> { };
            while (selectedExercises.Count < exerciseQuantity)
            {
                selectedExercises.Add(RandomlyChooseOneExercise(exerciseResults));
            }

            return selectedExercises;
        }

        //returns a randomly selected exercise, and removes the seleciton from the collection,
        //so it can't be chosen again.
        public static Exercise RandomlyChooseOneExercise(List<Exercise> exerciseResults)
        {
            int exerciseIndex = rand.Next(0, exerciseResults.Count);
            Exercise exercise = exerciseResults.ElementAt(exerciseIndex);
            exerciseResults.RemoveAt(exerciseIndex);
            
            return exercise;
        }

        public static int GetExerciseQty(LiftingComponent liftingComponent, int workoutMinutes, List<int> goalIds)
        {
            double singleExerciseTime = GetSingleExerciseTime(liftingComponent, goalIds);
            return (int)((workoutMinutes * 60) / singleExerciseTime);//convert workout minutes to seconds, then divide by seconds per exerise.
        }

        public static double GetSingleExerciseTime(LiftingComponent liftingComponent, List<int> goalIds)// return value is in seconds
        {
            // transitioning exercises takes energy. For goals where transition time is a large portion of the rest time, 
            // and extra time is needed to ACTUALLY rest, added time is figured into the overall workout duration.
            int exerciseTransitionTime = goalIds.Contains(3) || goalIds.Contains(4) ?
                muscEndTransitionTime : otherLiftTransitionTime; 
            return (liftingComponent.reps * repTime + liftingComponent.rest) * liftingComponent.sets + exerciseTransitionTime; 
        }

        public static double GetSingleExerciseTime(Exercise exercise)
        {
            // transitioning exercises takes energy. For goals where transition time is a large portion of the rest time, 
            // and extra time is needed to ACTUALLY rest, added time is figured into the overall workout duration.
            int exerciseTransitionTime = exercise.GoalId == 3 || exercise.GoalId == 4 ?
                muscEndTransitionTime : otherLiftTransitionTime;
            return (exercise.Reps * repTime + exercise.RestSeconds) * exercise.Sets + exerciseTransitionTime;
        }

        public static List<Exercise> AddLibrarytoExercises(List<Exercise> exercises, ExerciseLibrary exerciseLibrary)
        {
            for (int i = 0; i < exerciseLibrary.exercises.Length; i++)
            {
                Exercise tempResult = exerciseLibrary.exercises[i];
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
                if (!addedExerciseIds.Contains(exercise.exerciseId) && exercise.exerciseId != 393)//exercise 393 is trash. It's a full workout.
                {
                    revisedResults.Add(exercise);
                    addedExerciseIds.Add(exercise.exerciseId);
                }
            }
            return revisedResults;
        }

        //may be getting rid of this.
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
                    muscles = new int[] { 1, 2, 3, 4, 5, 9, 12, 13 };
                    break;
                case "Lower Body":
                    muscles = new int[] { 7, 8, 10, 11, 15 };
                    break;
                default:
                    muscles = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
                    break;
            }
            return muscles;
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

        public static string ConvertToMinSec(double input) //input in minutes. This does not round to nearest 15 seconds.
        {
            int minutes = (int)input;
            int seconds = (int)((input - minutes) * 60);
            StringBuilder newString = new StringBuilder("", expectedCardioStringMax);
            if (minutes != 0)
                newString.Append(minutes + " min ");
            if (seconds != 0)
                newString.Append(seconds + " sec");

            return newString.ToString();
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

        public static bool HasTwoLiftingGoals(ClientProgram currentProgram)
        {
            if (!CheckCardio(new List<int> { currentProgram.GoalOneId, currentProgram.GoalTwoId ?? 0})) 
                return true;

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

        public static TrainingStimulus SetTrainingStimulus(int goalId)
        {
            List<TrainingStimulus> trainingStimuli = SetTrainingStimuli(new List<int> { goalId });
            return trainingStimuli[0];
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
