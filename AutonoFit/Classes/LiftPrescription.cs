using AutonoFit.Contracts;
using AutonoFit.Models;
using AutonoFit.Services;
using AutonoFit.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public async Task<List<Exercise>> GatherExercises(SingleWorkoutVM workoutVM)
        {
            StringBuilder url = new StringBuilder(SharedUtility.BuildEquipmentUrlString(workoutVM.Equipment));
            List<Exercise> exercises = await FindExercisesByCategory(url, workoutVM.BodySection, new List<Exercise> { }); //Get exercises by category and repackage into Result reference type.
            await FindExercisesByMuscles(url, workoutVM.BodySection, exercises); //Get exercises by muslces and repackage into Result reference type.
            SharedUtility.RemoveRepeats(exercises); //Get rid of repeats
            return exercises;
        }

        public async Task<List<Exercise>> GatherExercises(List<ClientEquipment> equipment, string bodySection)
        {
            StringBuilder url = new StringBuilder(SharedUtility.BuildEquipmentUrlString(equipment));
            List<Exercise> exercises = await FindExercisesByCategory(url, bodySection, new List<Exercise> { }); //Get exercises by category and repackage into Result reference type.
            await FindExercisesByMuscles(url, bodySection, exercises); //Get exercises by muslces and repackage into Result reference type.
            SharedUtility.RemoveRepeats(exercises); //Get rid of repeats
            return exercises;
        }

        public async Task GenerateLiftingExercise(ClientProgram currentProgram, int todaysGoal, Exercise exercise)
        {
            TrainingStimulus trainingStimulus = SharedUtility.SetTrainingStimulus(todaysGoal);
            List<Exercise> pastExercises = await _repo.Exercise.GetExercisesByProgramAsync(currentProgram.ProgramId, exercise.id);
            //default reps and rest seconds if this will be the first or second time performing this exercise.
            exercise.Reps = trainingStimulus.minReps;
            exercise.RestSeconds = trainingStimulus.maxRestSeconds;

            if (pastExercises.Count >= 2)//exercise has been performed > 1 time in the past. It's possible to progress reps/rest.
                CheckLiftProgression(pastExercises, trainingStimulus, exercise);

            exercise.Sets = trainingStimulus.sets;
            exercise.RestString = SharedUtility.ConvertToMinSecString(exercise.RestSeconds);
        }

        private void CheckLiftProgression(List<Exercise> pastExercises, TrainingStimulus trainingStimulus, Exercise newExercise)
        {
            var past = pastExercises.OrderByDescending(c => c.id);
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


        public List<Exercise> ConvertOrderableToExercise(IOrderedEnumerable<Exercise> elements)
        {
            List<Exercise> exercises = new List<Exercise> { };
            foreach (var element in elements)
            {
                Exercise exercise = new Exercise();
                exercise.RestSeconds = element.RestSeconds;
                exercise.RPE = element.RPE;
                exercise.Reps = element.Reps;
                exercise.Sets = element.Sets;
                exercises.Add(exercise);
            }
            return exercises;
        }

        public async Task<List<Exercise>> FindExercisesByCategory(StringBuilder url, string upperOrLowerBody, List<Exercise> exercises)
        {
            ExerciseLibrary singleExerciseLibrary;
            int[] categories = SharedUtility.GetCategories(upperOrLowerBody);
            url.Append("&muscles=");

            for (int j = 0; j < categories.Length; j++)
            {
                url.Append(categories[j]).Append(",");
            }
            url.Remove(url.Length - 1, 1);

            singleExerciseLibrary = await _exerciseLibraryService.GetExercises(url.ToString());
            exercises = SharedUtility.AddLibrarytoExercises(exercises, singleExerciseLibrary);

            return exercises;
        }

        public async Task<List<Exercise>> FindExercisesByMuscles(StringBuilder url, string upperOrLowerBody, List<Exercise> exercises)
        {
            ExerciseLibrary singleExerciseLibrary;
            int[] muscles = SharedUtility.GetMuscles(upperOrLowerBody);
            url.Append("&muscles=");

            for (int j = 0; j < muscles.Length; j++)
            {
                url.Append(muscles[j]).Append(",");
            }
            url.Remove(url.Length -1, 1);

            singleExerciseLibrary = await _exerciseLibraryService.GetExercises(url.ToString());
            exercises = SharedUtility.AddLibrarytoExercises(exercises, singleExerciseLibrary);

            return exercises;
        }
    }
}
