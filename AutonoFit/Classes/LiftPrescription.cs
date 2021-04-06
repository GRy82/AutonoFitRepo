using AutonoFit.Contracts;
using AutonoFit.Models;
using AutonoFit.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.Classes
{
    public class LiftPrescription
    {
        private IRepositoryWrapper _repo;
        private ExerciseLibraryService _exerciseLibraryService;
        private const int repTime = 4;
        public LiftPrescription(IRepositoryWrapper repo, ExerciseLibraryService exerciseLibraryService)
        {
            _repo = repo;
            _exerciseLibraryService = exerciseLibraryService;
        }

        public int GetTodaysGoal(List<ClientWorkout> recentWorkoutCycle, ClientProgram currentProgram)
        {
            List<int> goalIds = new List<int> { currentProgram.GoalOneId, Convert.ToInt32(currentProgram.GoalTwoId) };
            if (currentProgram.GoalCount == 1)//If program has one goal, then return the only goal in the list that isn't 0.
            {
                return goalIds[1] == 0 ? goalIds[0] : goalIds[1];
            }
            else //Program has two goals. 
            {
                if (recentWorkoutCycle.Count == 0)//if no past workouts to go off of...
                {
                    return goalIds[0]; //arbitrarily start with first listed goal.  
                }
                else//past workouts available to check
                {
                    if (!goalIds.Contains(4) && !goalIds.Contains(5))//if only lifting goals, only alternate goals once you have two consecutive lifts of same goal, one UB, one LB.
                    {
                        if (recentWorkoutCycle.Count == 1 || recentWorkoutCycle[0].GoalId != recentWorkoutCycle[1].GoalId)
                        {
                            return Convert.ToInt32(recentWorkoutCycle[0].GoalId); //return same goal as last workout.
                        }
                        else if (recentWorkoutCycle[0].GoalId == recentWorkoutCycle[1].GoalId)
                        {
                            return goalIds[0] == recentWorkoutCycle[0].GoalId ? goalIds[1] : goalIds[0]; //return the other goal.
                        }
                    }
                    // else cardio goals are present

                    return recentWorkoutCycle[0].GoalId == goalIds[0] ? goalIds[1] : goalIds[0]; //return the goal that is not that of the last workout.        
                }
            }
        }

        public string GetBodyParts(List<ClientWorkout> recentWorkoutCycle, int todaysGoalNumber, int goalCount)
        {
            if (recentWorkoutCycle.Count == 0 || (goalCount == 2 && recentWorkoutCycle.Count == 1))
            {
                return "Upper Body"; // this is the first workout of the program, or the first of its kind. Arbitrarily start with upper body.
            }
            return recentWorkoutCycle[0].BodyParts == "Upper Body" ? "Lower Body" : "Upper Body"; //always can alternate the body parts. 
        }

        //Move this, and its helper methods to Prescription class eventually/
        public async Task<ClientExercise> GenerateLiftingExercise(ClientProgram currentProgram, int todaysGoal, int exerciseId)
        {
            TrainingStimulus trainingStimulus = SharedUtility.SetTrainingStimulus(todaysGoal);
            List<ClientExercise> pastExercises = await _repo.ClientExercise.GetClientExercisesByProgramAsync(currentProgram.ProgramId, exerciseId);
            ClientExercise newExercise = new ClientExercise();
            //default reps and rest seconds if this will be the first or second time performing this exercise.
            newExercise.Reps = trainingStimulus.minReps;
            newExercise.RestSeconds = trainingStimulus.maxRestSeconds;

            if (pastExercises.Count >= 2)//exercise has been performed > 1 time. Now possible to progress reps/rest.
                CheckLiftProgression(pastExercises, trainingStimulus, newExercise);

            newExercise.Sets = trainingStimulus.sets;
            newExercise.RestString = SharedUtility.ConvertToMinSecString(newExercise.RestSeconds);

            return newExercise;
        }

        private void CheckLiftProgression(List<ClientExercise> pastExercises, TrainingStimulus trainingStimulus, ClientExercise newExercise)
        {
            var past = pastExercises.OrderByDescending(c => c.Id);
            pastExercises = ConvertOrderableToExercise(past);

            newExercise.Reps = pastExercises[0].Reps;
            newExercise.RestSeconds = pastExercises[0].RestSeconds;

            if (pastExercises[0].RPE < pastExercises[1].RPE && pastExercises[0].Reps == pastExercises[1].Reps)
            {
                newExercise.Reps = pastExercises[0].Reps + 1 > trainingStimulus.maxReps ? trainingStimulus.minReps : pastExercises[0].Reps + trainingStimulus.repsInterval;
                newExercise.RestSeconds = pastExercises[0].RestSeconds - trainingStimulus.restInterval < trainingStimulus.minRestSeconds ?
                    trainingStimulus.maxRestSeconds : pastExercises[0].RestSeconds - trainingStimulus.restInterval;
            }
        }


        public List<ClientExercise> ConvertOrderableToExercise(IOrderedEnumerable<ClientExercise> elements)
        {
            List<ClientExercise> exercises = new List<ClientExercise> { };
            foreach (var element in elements)
            {
                ClientExercise exercise = new ClientExercise();
                exercise.RestSeconds = element.RestSeconds;
                exercise.RPE = element.RPE;
                exercise.Reps = element.Reps;
                exercise.Sets = element.Sets;
                exercises.Add(exercise);

            }
            return exercises;
        }

        public async Task<List<Exercise>> FindExercisesByCategory(List<ClientEquipment> equipment, string upperOrLowerBody, List<Exercise> exercises)
        {
            ExerciseLibrary exerciseLibrary;
            int[] categories = SharedUtility.GetCategories(upperOrLowerBody);
            for (int i = 0; i < categories.Length; i++)
            {
                string urlCategoryString = SharedUtility.BuildEquipmentUrlString(equipment) + "&category=" + categories[i];
                exerciseLibrary = await _exerciseLibraryService.GetExercises(urlCategoryString);
                exercises = SharedUtility.AddLibrarytoExercises(exercises, exerciseLibrary);
            }

            return exercises;
        }

        public async Task<List<Exercise>> FindExercisesByMuscles(List<ClientEquipment> equipment, string upperOrLowerBody, List<Exercise> exercises)
        {
            ExerciseLibrary singleExerciseLibrary;
            int[] muscles = SharedUtility.GetMuscles(upperOrLowerBody);
            string urlMusclesString = null;
            for (int j = 0; j < muscles.Length; j++)
            {
                urlMusclesString += "&muscles=" + muscles[j];
            }
            urlMusclesString = SharedUtility.BuildEquipmentUrlString(equipment) + urlMusclesString;
            singleExerciseLibrary = await _exerciseLibraryService.GetExercises(urlMusclesString);
            exercises = SharedUtility.AddLibrarytoExercises(exercises, singleExerciseLibrary);

            return exercises;
        }
    }
}
