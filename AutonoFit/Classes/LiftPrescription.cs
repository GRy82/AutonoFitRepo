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
        public string GetBodyParts(string lastWorkoutBodyParts, int todaysGoalNumber, ClientProgram currentProgram)
        {
            if (lastWorkoutBodyParts == null)
                return "Upper Body"; //arbitrarily start with upper body in new program.

            bool lowerBodyNeedsRest = lastWorkoutBodyParts == "Both" || lastWorkoutBodyParts == "Lower Body";
            if (lowerBodyNeedsRest)
                return "Upper Body";

            return "Lower Body";
        }

        public async Task<List<Exercise>> GatherExercises(SingleWorkoutVM workoutVM)
        {
            StringBuilder url = new StringBuilder(SharedUtility.BuildEquipmentUrlString(workoutVM.Equipment));
            List<Exercise> exercises = await FindExercisesByMuscles(new StringBuilder(url.ToString()), workoutVM.BodySection, new List<Exercise> { }); //Get exercises by category and repackage into Result reference type.
            //wger may no longer have an endpoint for category disallowing for the identification of waht the categories actually mean.
            //await FindExercisesByCategory(url, workoutVM.BodySection, exercises); 
            return exercises;
        }

        public async Task<List<Exercise>> GatherExercises(List<ClientEquipment> equipment, string bodySection)
        {
            StringBuilder url = new StringBuilder(SharedUtility.BuildEquipmentUrlString(equipment));
            List<Exercise> exercises = await FindExercisesByMuscles(new StringBuilder(url.ToString()), bodySection, new List<Exercise> { }); //Get exercises by category and repackage into Result reference type
            //wger may no longer have an endpoint for category which disallows for the identification of waht the categories actually mean.
            //await FindExercisesByCategory(url, bodySection, exercises); 
            exercises = SharedUtility.RemoveRepeats(exercises); //Get rid of repeats
            return exercises;
        }

        //May be getting rid of this.
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
            List<Exercise> previouslyPerformed = await _repo.Exercise.GetDiffExercisesByProgramGoalAsync(currentProgram.ProgramId, todaysGoalNumber);
            liftingComponent.exercises = await AddExercisesToComponent(totalExercises, new List<Exercise>(), previouslyPerformed, clientWorkout,
                                                                    currentProgram, todaysGoalNumber, liftWorkoutInMinutes, liftWorkoutInMinutes);
            CleanseExerciseDescriptions(liftingComponent.exercises);

            return liftingComponent;
        }

        private async Task<List<Exercise>> AddExercisesToComponent(List<Exercise> totalExercises, List<Exercise> chosenExercises,   
                                 List<Exercise> previouslyPerformed, ClientWorkout clientWorkout,  ClientProgram currentProgram,
                                                            int todaysGoalNumber,  int availableMinutes, int liftWorkoutMinutes)
        {
            if (availableMinutes <= 0) return chosenExercises;

            Exercise newExercise = SelectOneExercise(totalExercises, previouslyPerformed, availableMinutes, liftWorkoutMinutes);
            await AssignPropertiesToExercise(newExercise, clientWorkout, currentProgram, todaysGoalNumber);
            chosenExercises.Add(newExercise);
            availableMinutes -= (int)Math.Round(SharedUtility.GetSingleExerciseTime(newExercise) / 60);

            return await AddExercisesToComponent(totalExercises, chosenExercises, previouslyPerformed,
                clientWorkout, currentProgram, todaysGoalNumber, availableMinutes, liftWorkoutMinutes);
        }

        //Will randomly choose from exercises that have been done before until those repeated exercises account for half the workout duration.
        //At that point, exercises will be picked at random from a larger pool of exercises--those that have been previously performed,
        //and those that have not been. This promotes variety and continuity simultaneously.  
        public Exercise SelectOneExercise(List<Exercise> totalExercises, List<Exercise> previouslyPerformed, int availableMinutes, int liftWorkoutMinutes)
        {
            if(availableMinutes <= liftWorkoutMinutes / 2 || previouslyPerformed.Count == 0)
                return SharedUtility.RandomlyChooseOneExercise(totalExercises);
   
            var exercise = SharedUtility.RandomlyChooseOneExercise(previouslyPerformed);

            //You still want to use the exercise from totalExercises, because that contains a description
            // property whereas an exercise from previouslyPerformed does not.
            var matchingExercise = totalExercises.Find(c => c.exerciseId == exercise.exerciseId);
            totalExercises.Remove(matchingExercise);

            return matchingExercise;
        }

        public async Task AssignPropertiesToExercise(Exercise exercise, ClientWorkout clientWorkout, ClientProgram currentProgram, int todaysGoalNumber)
        {
            await SetLiftingExerciseParameters(currentProgram, todaysGoalNumber, exercise);
            var client = await _repo.Client.GetClientAsync(clientWorkout.ClientId);//Check this
            exercise.GoalId = todaysGoalNumber;
            exercise.ClientId = client.ClientId;
            exercise.WorkoutId = clientWorkout.Id;
            exercise.ProgramId = currentProgram.ProgramId;
        }

        public async Task SetLiftingExerciseParameters(ClientProgram currentProgram, int todaysGoal, Exercise exercise)
        {
            TrainingStimulus trainingStimulus = SharedUtility.SetTrainingStimulus(todaysGoal);
            List<Exercise> repeatPerformances = await _repo.Exercise.GetSameExercisesByProgramGoalAsync(currentProgram.ProgramId, 
                                                                                                    exercise.exerciseId, todaysGoal);
            //default reps and rest seconds if this will be the first or second time performing this exercise.
            exercise.Reps = trainingStimulus.minReps;
            exercise.RestSeconds = trainingStimulus.maxRestSeconds;

            if (repeatPerformances.Count > 1)//exercise has been performed > 1 time in the past. It's possible to progress reps/rest.
                CheckLiftProgression(repeatPerformances, trainingStimulus, exercise);

            exercise.Sets = trainingStimulus.sets;
            exercise.RestString = SharedUtility.ConvertToMinSecString(exercise.RestSeconds);
        }

        public void CheckLiftProgression(List<Exercise> repeatPerformances, TrainingStimulus trainingStimulus, Exercise newExercise)
        {
            var past = repeatPerformances.OrderByDescending(c => c.Id);
            repeatPerformances = ConvertOrderableToExercise(past);

            newExercise.Reps = repeatPerformances[0].Reps;
            newExercise.RestSeconds = repeatPerformances[0].RestSeconds;
            //TO progress parameters, RPE must have decreased, last workout reps must be >= to second last. (Goals must be the same, but this is already guaranteed.)
            if (repeatPerformances[0].RPE < repeatPerformances[1].RPE && repeatPerformances[0].Reps >= repeatPerformances[1].Reps)
            {
                newExercise.Reps = repeatPerformances[0].Reps + 1 > trainingStimulus.maxReps ? 
                    trainingStimulus.minReps : repeatPerformances[0].Reps + trainingStimulus.repsInterval;
                newExercise.RestSeconds = repeatPerformances[0].RestSeconds - trainingStimulus.restInterval < trainingStimulus.minRestSeconds ?
                    trainingStimulus.maxRestSeconds : repeatPerformances[0].RestSeconds - trainingStimulus.restInterval;
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
                exercise.description = exercise.description == null ? "No description provided from database." 
                    : SharedUtility.RemoveTags(exercise.description);           
        }
    }
}
