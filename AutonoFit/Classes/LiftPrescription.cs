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
        private ProgramModule programModule;
        private const int repTime = 4;
        public LiftPrescription(IRepositoryWrapper repo, ExerciseLibraryService exerciseLibraryService)
        {
            _repo = repo;
            _exerciseLibraryService = exerciseLibraryService;
            programModule = new ProgramModule(repo);
        }

        public async Task<int> GetTodaysGoal(ClientProgram currentProgram)
        {
            if (currentProgram.GoalCount == 1)
                return currentProgram.GoalOneId;
            // else GoalCount == 2
            int workoutsCompleted = await programModule.GetWorkoutsCompletedByProgram(currentProgram.ProgramId);
            if (!SharedUtility.HasTwoLiftingGoals(currentProgram))// Has 1 cardio goal, 1 lifting goal. Just alternate between goals.
                return workoutsCompleted % 2 == 0 ? currentProgram.GoalOneId : (int)currentProgram.GoalTwoId;
            //Has 2 lifting goals. Goals remain the same for 2 workouts in a row. See chart below to make sense of following code.                                  
            if (workoutsCompleted % 2 == 1)
                workoutsCompleted++;
            return workoutsCompleted % 4 == 0 ? (int)currentProgram.GoalTwoId : currentProgram.GoalOneId;
            
            //         workoutsCompleted : 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 |
            //                     Goal# : 1 | 1 | 2 | 2 | 1 | 1 | 2 | 2 | 
            // (Upper/Lower)Body Section : U | L | U | L | U | L | U | L | *** Alternating Upper/lower body is what's healthy/important.
        }

        //Only called if a lift is needed.
        //Body part selection of Supplemental lifts is already accounted for. Do not worry about it here.
        public string GetBodyParts(List<ClientWorkout> recentWorkoutCycle, int todaysGoalNumber, ClientProgram currentProgram)
        {
            if (recentWorkoutCycle.Count == 0)
                return "Upper Body"; //arbitrarily start with upper body in new program.
            bool lowerBodyNeedsRest = recentWorkoutCycle[0].BodyParts == "Both" || recentWorkoutCycle[0].BodyParts == "Lower Body";

            if (lowerBodyNeedsRest)
                return "Upper Body";

            return "Lower Body";
        }

        public async Task<List<Exercise>> GatherExercises(SingleWorkoutVM workoutVM)
        {
            StringBuilder url = new StringBuilder(SharedUtility.BuildEquipmentUrlString(workoutVM.Equipment));
            List<Exercise> exercises = await FindExercisesByCategory(new StringBuilder(url.ToString()), workoutVM.BodySection, new List<Exercise> { }); //Get exercises by category and repackage into Result reference type.
            await FindExercisesByMuscles(url, workoutVM.BodySection, exercises); //Get exercises by muslces and repackage into Result reference type.
            SharedUtility.RemoveRepeats(exercises); //Get rid of repeats
            return exercises;
        }

        public async Task<List<Exercise>> GatherExercises(List<ClientEquipment> equipment, string bodySection)
        {
            StringBuilder url = new StringBuilder(SharedUtility.BuildEquipmentUrlString(equipment));
            List<Exercise> exercises = await FindExercisesByCategory(new StringBuilder(url.ToString()), bodySection, new List<Exercise> { }); //Get exercises by category and repackage into Result reference type
            await FindExercisesByMuscles(url, bodySection, exercises); //Get exercises by muslces and repackage into Result reference type.
            SharedUtility.RemoveRepeats(exercises); //Get rid of repeats
            return exercises;
        }

        public async Task GenerateLiftingExercise(ClientProgram currentProgram, int todaysGoal, Exercise exercise)
        {
            TrainingStimulus trainingStimulus = SharedUtility.SetTrainingStimulus(todaysGoal);
            List<Exercise> pastExercises = await _repo.Exercise.GetExercisesByProgramAsync(currentProgram.ProgramId, exercise.exerciseId);
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
            url.Append("&category=");

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
