using AutonoFit.Contracts;
using AutonoFit.Models;
using AutonoFit.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.Classes
{
    public class ProgramModule
    {
        private readonly IRepositoryWrapper _repo;
        public const int repTime = 4;

        public ProgramModule(IRepositoryWrapper repo)
        {
            _repo = repo;
        }

        public async Task<bool> ProgramNameTaken(string programName, int clientId)
        {
            List<ClientProgram> programs = await _repo.ClientProgram.GetAllClientProgramsAsync(clientId); 

            foreach (ClientProgram program in programs)
            {
                if (program.ProgramName == programName)
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<int> GetWorkoutsCompletedByProgram(int programId)
        {
            int totalWorkoutCount = 0;
          
            List<ClientWorkout> workouts = await _repo.ClientWorkout.GetAllWorkoutsByProgramAsync(programId);
            foreach(ClientWorkout workout in workouts)
            {
                if (workout.Completed == true)
                {
                    totalWorkoutCount++;
                }
            }
            
            return totalWorkoutCount;
        }

        public async Task<double> CalculateAttendanceRating(int programId, int workoutsCompleted)
        {
            double attendanceRating = 0;
            ClientProgram clientProgram = await _repo.ClientProgram.GetClientProgramAsync(programId);
            TimeSpan timeSinceProgramStart = DateTime.Now - clientProgram.ProgramStart;
            int programLengthDays = timeSinceProgramStart.Days < 1 ? 1 : timeSinceProgramStart.Days;
            double weeks = programLengthDays / 7 < 1 ? 1 : programLengthDays / 7;
            attendanceRating = workoutsCompleted / (clientProgram.DaysPerWeek * Math.Round(weeks));

            return attendanceRating * 100;
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
                if(recentWorkoutCycle.Count == 0)//if no past workouts to go off of...
                {
                    return goalIds[0]; //arbitrarily start with first listed goal.  
                }
                else//past workouts available to check
                {
                    if(!goalIds.Contains(4) && !goalIds.Contains(5))//if only lifting goals, only alternate goals once you have two consecutive lifts of same goal, one UB, one LB.
                    {
                        if (recentWorkoutCycle.Count == 1 || recentWorkoutCycle[0].GoalId != recentWorkoutCycle[1].GoalId)
                        {
                            return Convert.ToInt32(recentWorkoutCycle[0].GoalId); //return same goal as last workout.
                        }
                        else if(recentWorkoutCycle[0].GoalId == recentWorkoutCycle[1].GoalId)
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
            if(recentWorkoutCycle.Count == 0 || (goalCount == 2 && recentWorkoutCycle.Count == 1))
            {
                return "Upper Body"; // this is the first workout of the program, or the first of its kind. Arbitrarily start with upper body.
            }
            return recentWorkoutCycle[0].BodyParts == "Upper Body" ? "Lower Body" : "Upper Body"; //always can alternate the body parts. 
        }

        public async Task<FitnessParameters> GenerateLift(ClientProgram currentProgram, List<ClientWorkout> recentWorkoutCycle, FitnessParameters fitnessMetrics, int todaysGoal, int exerciseId)
        {
            List<TrainingStimulus> trainingStimulus = SharedUtility.DefineTrainingStimuli(new List<int> { todaysGoal });
            List<ClientExercise> pastExercises = await _repo.ClientExercise.GetClientExercisesByProgramAsync(currentProgram.ProgramId, exerciseId);

            if (pastExercises.Count <= 1)//start at min rep, max rest.
            {
                fitnessMetrics.reps = trainingStimulus[0].minReps;
                fitnessMetrics.rest = trainingStimulus[0].maxRestSeconds;
            }
            else if (pastExercises.Count >= 2)
            {
                var past = pastExercises.OrderByDescending(c => c.Id);
                pastExercises = ConvertOrderableToExercise(past);

                if (pastExercises[0].RPE < pastExercises[1].RPE && pastExercises[0].Reps == pastExercises[1].Reps) 
                {
                    fitnessMetrics.reps = pastExercises[0].Reps + 1 > trainingStimulus[0].maxReps ? trainingStimulus[0].minReps : pastExercises[0].Reps + trainingStimulus[0].repsInterval;
                    fitnessMetrics.rest = pastExercises[0].RestSeconds - trainingStimulus[0].restInterval < trainingStimulus[0].minRestSeconds ? trainingStimulus[0].maxRestSeconds : pastExercises[0].RestSeconds - trainingStimulus[0].restInterval;
                }
                else
                {
                    fitnessMetrics.reps = pastExercises[0].Reps;
                    fitnessMetrics.rest = pastExercises[0].RestSeconds;
                }
            }
          
            fitnessMetrics.sets = trainingStimulus[0].sets;
            fitnessMetrics.restString = SharedUtility.ConvertToMinSecString(fitnessMetrics.rest);

            return fitnessMetrics;
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

    }
}
