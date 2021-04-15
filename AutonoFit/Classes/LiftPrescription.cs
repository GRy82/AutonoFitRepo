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

        public LiftPrescription()
        {

        }
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
            exercises = SharedUtility.RemoveRepeats(exercises); //Get rid of repeats
            return exercises;
        }

        public async Task<List<Exercise>> GatherExercises(List<ClientEquipment> equipment, string bodySection)
        {
            StringBuilder url = new StringBuilder(SharedUtility.BuildEquipmentUrlString(equipment));
            List<Exercise> exercises = await FindExercisesByCategory(new StringBuilder(url.ToString()), bodySection, new List<Exercise> { }); //Get exercises by category and repackage into Result reference type
            await FindExercisesByMuscles(url, bodySection, exercises); //Get exercises by muslces and repackage into Result reference type.
            exercises = SharedUtility.RemoveRepeats(exercises); //Get rid of repeats
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
            url.Remove(url.Length - 1, 1);

            singleExerciseLibrary = await _exerciseLibraryService.GetExercises(url.ToString());
            exercises = SharedUtility.AddLibrarytoExercises(exercises, singleExerciseLibrary);

            return exercises;
        }

        public async Task<LiftingComponent> GenerateLiftingComponent(string upperOrLowerBody, int todaysGoalNumber, ClientWorkout clientWorkout,
                                                        ClientProgram currentProgram, int liftWorkoutInMinutes, List<ClientEquipment> equipment)
        {
            LiftingComponent liftingComponent = new LiftingComponent(SharedUtility.SetTrainingStimuli(new List<int> { todaysGoalNumber }));
            List<Exercise> totalExercises = await GatherExercises(equipment, upperOrLowerBody);//Gets all eligible exercises, and no repeats.
            liftingComponent.exercises = await GenerateLiftingComponent(totalExercises, new List<Exercise>(), liftWorkoutInMinutes,
                                                                        clientWorkout, currentProgram, todaysGoalNumber);
            CleanseExerciseDescriptions(liftingComponent.exercises);

            return liftingComponent;
        }

        private async Task<List<Exercise>> GenerateLiftingComponent(List<Exercise> totalExercises, List<Exercise> chosenExercises, int liftWorkoutInMinutes,
                                                                            ClientWorkout clientWorkout, ClientProgram currentProgram, int todaysGoalNumber)
        {
            if (liftWorkoutInMinutes <= 0) return chosenExercises;

            Exercise newExercise = SharedUtility.RandomlyChooseOneExercise(totalExercises);
            await AssignPropertiesToExercise(newExercise, clientWorkout, currentProgram, todaysGoalNumber);
            chosenExercises.Add(newExercise);
            liftWorkoutInMinutes -= (int)Math.Round(SharedUtility.GetSingleExerciseTime(newExercise) / 60);

            return await GenerateLiftingComponent(totalExercises, chosenExercises, liftWorkoutInMinutes, clientWorkout, currentProgram, todaysGoalNumber);
        }

        public async Task AssignPropertiesToExercise(Exercise exercise, ClientWorkout clientWorkout, ClientProgram currentProgram, int todaysGoalNumber)
        {
            await GenerateLiftingExercise(currentProgram, todaysGoalNumber, exercise);
            var client = await _repo.Client.GetClientAsync(clientWorkout.ClientId);//Check this
            exercise.ClientId = client.ClientId;
            exercise.WorkoutId = clientWorkout.Id;
            exercise.ProgramId = currentProgram.ProgramId;
        }

        public async Task GenerateLiftingExercise(ClientProgram currentProgram, int todaysGoal, Exercise exercise)
        {
            TrainingStimulus trainingStimulus = SharedUtility.SetTrainingStimulus(todaysGoal);
            List<Exercise> pastExercises = await _repo.Exercise.GetExercisesByProgramAsync(currentProgram.ProgramId, exercise.exerciseId);
            //default reps and rest seconds if this will be the first or second time performing this exercise.
            exercise.Reps = trainingStimulus.minReps;
            exercise.RestSeconds = trainingStimulus.maxRestSeconds;

            if (pastExercises.Count >= 1)//exercise has been performed > 1 time in the past. It's possible to progress reps/rest.
                CheckLiftProgression(pastExercises, trainingStimulus, exercise);

            exercise.Sets = trainingStimulus.sets;
            exercise.RestString = SharedUtility.ConvertToMinSecString(exercise.RestSeconds);
        }

        public void CheckLiftProgression(List<Exercise> pastExercises, TrainingStimulus trainingStimulus, Exercise newExercise)
        {
            var past = pastExercises.OrderByDescending(c => c.Id);
            pastExercises = ConvertOrderableToExercise(past);

            newExercise.Reps = pastExercises[0].Reps;
            newExercise.RestSeconds = pastExercises[0].RestSeconds;
            //TO progress parameters, RPE must have decreased, last workout reps must be >= to second last. (Goals must be the same, but this is already guaranteed.)
            if (pastExercises[0].RPE < pastExercises[1].RPE && pastExercises[0].Reps >= pastExercises[1].Reps)
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

        private void CleanseExerciseDescriptions(List<Exercise> exercises)
        {
            foreach (Exercise exercise in exercises)
                exercise.description = SharedUtility.RemoveTags(exercise.description);
        }
    }
}
