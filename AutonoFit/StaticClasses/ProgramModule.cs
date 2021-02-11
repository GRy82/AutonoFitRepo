using AutonoFit.Contracts;
using AutonoFit.Models;
using AutonoFit.StaticClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.Classes
{
    public class ProgramModule
    {
        private readonly IRepositoryWrapper _repo;
        public static int repTime = 4;

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


        public int GetTodaysGoal(List<ClientWorkout> recentWorkoutCycle, List<int> goalIds, int goalCount)
        {
            if (goalCount == 1)//If program has one goal, then return the only goal in the list that isn't 0.
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

        public async Task<FitnessDictionary> GetTodaysCardio(FitnessDictionary fitnessMetrics, List<ClientWorkout> recentWorkoutCycle, int todaysGoalNumber, ClientProgram currentProgram)
        {
            string runType = null;
            if(currentProgram.GoalCount == 1 && currentProgram.DaysPerWeek == 6)
            {
                recentWorkoutCycle = SanitizeWorkouts(recentWorkoutCycle);
            }
            
            if(currentProgram.GoalCount == 1 && recentWorkoutCycle.Count != 0)
            {
                runType = AdvanceRunType(recentWorkoutCycle[0].RunType);
            }
            else if (currentProgram.GoalCount == 2 && recentWorkoutCycle.Count > 1)
            {
                runType = AdvanceRunType(recentWorkoutCycle[1].RunType);
            }
            else //this is the first run, or first workout
            {
                runType = "Easy";
            }
            //Corner Case
            if(runType == "Easy" && currentProgram.GoalCount == 1 && currentProgram.DaysPerWeek == 6) // == easy after the advancement.
            {
                fitnessMetrics.runType = "6 Lift"; //this code will indicate a lift should be done instead of a run.  
                return fitnessMetrics;//returned with only one change.
            }

            fitnessMetrics.cardio = true;
            fitnessMetrics = await GenerateRun(currentProgram, runType, fitnessMetrics, recentWorkoutCycle);
            fitnessMetrics = SharedUtility.ConvertFitnessDictCardioValues(fitnessMetrics);

            return fitnessMetrics;
        }


        private async Task<FitnessDictionary> GenerateRun(ClientProgram currentProgram, string runType, FitnessDictionary fitnessMetrics, List<ClientWorkout> recentWorkoutCycle)
        {
            double paceCoefficient = GetPaceCoefficient(runType);
            if((currentProgram.GoalCount == 1 && recentWorkoutCycle.Count > 1) || (currentProgram.GoalCount == 2 && recentWorkoutCycle.Count > 3))
            {
                await CheckCardioProgression(currentProgram, recentWorkoutCycle);
            }
           
            fitnessMetrics.milePace = (double)currentProgram.MileMinutes + (Convert.ToDouble(currentProgram.MileSeconds) / 60);
            fitnessMetrics.milePace *= paceCoefficient;
            fitnessMetrics.runDuration = currentProgram.MinutesPerSession;
            fitnessMetrics.distanceMiles = fitnessMetrics.runDuration / fitnessMetrics.milePace;
            fitnessMetrics.runType = runType;
            if (runType == "Easy")//all easy runs will need time to be accompanied by an aerobic lifting workout.
            {
                fitnessMetrics.runDuration /= 2;
            }

            return fitnessMetrics;
        }

        public async Task CheckCardioProgression(ClientProgram currentProgram, List<ClientWorkout> recentWorkoutCycle)
        {
            bool readyToProgress = (currentProgram.GoalCount == 1 && recentWorkoutCycle[0].CardioRPE < recentWorkoutCycle[1].CardioRPE) ||
                (currentProgram.GoalCount == 2 && recentWorkoutCycle[1].CardioRPE < recentWorkoutCycle[3].CardioRPE);
           
            if (readyToProgress)
            {
                if (currentProgram.MileSeconds == 0)
                {
                    currentProgram.MileMinutes -= 1;
                    currentProgram.MileSeconds = 59;
                }
                else
                {
                    currentProgram.MileSeconds -= 1;
                }
                _repo.ClientProgram.EditClientProgram(currentProgram);
                await _repo.SaveAsync();
            }
        }

        public string AdvanceRunType(string runType)
        {
            if(runType == "6 Lift")//This is a corner case. See calling method for reasoning.
            {
                return "Easy";
            }
            string[] runTypes = new string[] { "Easy", "Moderate", "Long", "Speed" };
            int index = runType.IndexOf("runType");
            string newRunType = index != (runTypes.Length - 1) ? runTypes[index + 1] : runTypes[0] ;

            return newRunType;
        }

        public double GetPaceCoefficient(string runType)
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

        public List<ClientWorkout> SanitizeWorkouts(List<ClientWorkout> recentWorkoutCycle)//Makes sure that no "6 Lift" exercises make it in the collection.
        {
            List<ClientWorkout> sanitizedWorkouts = new List<ClientWorkout> { };
            foreach(ClientWorkout workout in recentWorkoutCycle)
            {
                if (workout.GoalId < 4 || (workout.GoalId > 3 && workout.RunType != null))
                {
                    sanitizedWorkouts.Add(workout);
                }
            }
            return sanitizedWorkouts;
        }

        public async Task<FitnessDictionary> GenerateLift(ClientProgram currentProgram, List<ClientWorkout> recentWorkoutCycle, FitnessDictionary fitnessMetrics, int todaysGoal, int exerciseId)
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
                var past = from s in pastExercises
                                orderby s.Id descending
                                select s; //use date to order this, if i ever use hash values for id instead.
                pastExercises = ConvertVarToExercise(past);
                if (pastExercises[0].RPE < pastExercises[1].RPE) 
                {
                    fitnessMetrics.reps = pastExercises[0].Reps + 1 > trainingStimulus[0].maxReps ? trainingStimulus[0].minReps : pastExercises[0].Reps + 1;
                    fitnessMetrics.rest = pastExercises[0].RestSeconds - trainingStimulus[0].restInterval < trainingStimulus[0].maxRestSeconds ? trainingStimulus[0].maxRestSeconds : pastExercises[0].RestSeconds - trainingStimulus[0].restInterval;
                }
            }
          
            fitnessMetrics.sets = trainingStimulus[0].sets;
            fitnessMetrics.restString = SharedUtility.ConvertToMinSecString(fitnessMetrics.rest);

            return fitnessMetrics;
        }


        public List<ClientExercise> ConvertVarToExercise(IOrderedEnumerable<ClientExercise> elements)
        {
            List<ClientExercise> exercises = new List<ClientExercise> { };
            foreach (var element in elements)
            {
                ClientExercise exercise = new ClientExercise();
                exercise.RestSeconds = element.RestSeconds;
                exercise.RPE = element.RPE;
                exercise.Reps = element.Reps;
                exercises.Add(exercise);

            }
            return exercises;
        }

    }
}
