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
            List<ClientWeek> clientWeeks = await _repo.ClientWeek.GetAllClientWeeksAsync(programId); 

            foreach(ClientWeek week in clientWeeks)
            {
                List<ClientWorkout> workouts = await _repo.ClientWorkout.GetAllWorkoutsByWeekAsync(week.Id);
                foreach(ClientWorkout workout in workouts)
                {
                    if (workout.Completed == true)
                    {
                        totalWorkoutCount++;
                    }
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
            else //Program has two goals. Whatever the last workout's goal was, alternate the goal for today's workout.
            {
                if(recentWorkoutCycle.Count == 0)//if no past workouts to go off of...
                {
                    return goalIds[0]; //arbitrarily start with first listed goal.  
                }
                else
                {
                    return recentWorkoutCycle[0].GoalId == goalIds[0] ? goalIds[1] : goalIds[0];
                }
            }
        }

        public string GetBodyParts(List<ClientWorkout> recentWorkoutCycle, int todaysGoalNumber, int goalCount, bool cardio = false)
        {
            if (goalCount == 1)//one goal of program
            {
                if (recentWorkoutCycle.Count > 0)
                {
                    return recentWorkoutCycle[0].BodyParts == "Upper Body" ? "Lower Body" : "Upper Body";
                }
                else
                {
                    return "Upper Body"; // No past workouts to go off of, return "Upper Body arbitrarily"
                }
            }
            else //two goals of program.
            { 
                if(recentWorkoutCycle.Count <= 1) //meaning this workout is either the first one, or the first of its kind
                {
                    return "Upper Body"; // No past workouts to go off of, return "Upper Body arbitrarily"
                }
                else // alternate body parts with the second last workout, which would be the last workout of the same type.
                {
                    return recentWorkoutCycle[1].BodyParts == "Upper Body" ? "Lower Body" : "Upper Body";
                }
            }
        }
    
        public FitnessDictionary GetTodaysCardio(FitnessDictionary fitnessMetrics, List<ClientWorkout> recentWorkoutCycle, int todaysGoalNumber, ClientProgram currentProgram)
        {
            List<string> alternationArray = new List<string> { };
            int index;
            if (recentWorkoutCycle.Count == 0 || (recentWorkoutCycle.Count == 1 && currentProgram.GoalCount == 2))//if it's the first run of the program
            {
                fitnessMetrics = GetArbitraryStart(currentProgram, fitnessMetrics);
            }
            else // not the first run, alternate/periodize workouts
            {
                if (currentProgram.GoalCount == 2)
                {
                    if (currentProgram.DaysPerWeek == 6)
                    {
                        alternationArray = new List<string> { "Easy", "Long", "Speed" };
                    }
                    else if (currentProgram.DaysPerWeek > 3 && currentProgram.DaysPerWeek < 6)
                    {
                        alternationArray = new List<string> { "Easy", "Long"};
                    }
                    else
                    {
                        alternationArray = new List<string> { "Moderate" };
                    }
                    index = alternationArray.IndexOf(recentWorkoutCycle[1].RunType);//check second last workout for same goal type
                }
                else
                {
                    if (currentProgram.DaysPerWeek == 6)
                    {
                        alternationArray = new List<string> { "Easy", "Long", "Speed" };//Don't let them run 6 times a week. Have muscular endurance workout
                    }
                    else if (currentProgram.DaysPerWeek > 3 && currentProgram.DaysPerWeek < 6) 
                    {
                        alternationArray = new List<string> { "Easy", "Long", "Speed" };
                    }
                    else if (currentProgram.DaysPerWeek == 3)
                    {
                        alternationArray = new List<string> { "Easy", "Moderate", "Long" };
                    }
                    else
                    {
                        alternationArray = new List<string> { "Easy", "Long" };
                    }

                    index = alternationArray.IndexOf(recentWorkoutCycle[0].RunType); // check last run, which will be first indexed, ie. [0].
                }

                fitnessMetrics = index == (alternationArray.Count - 1) ? GenerateRun(currentProgram, alternationArray[0], fitnessMetrics) : GenerateRun(currentProgram, alternationArray[index + 1], fitnessMetrics);
            }

            return fitnessMetrics;
        }

        private FitnessDictionary GetArbitraryStart(ClientProgram currentProgram, FitnessDictionary fitnessMetrics)
        {
            if(currentProgram.GoalCount == 1)
            {
                fitnessMetrics = GenerateRun(currentProgram, "Easy", fitnessMetrics);
            }
            else
            {
                if(currentProgram.DaysPerWeek < 4)
                {
                    fitnessMetrics = GenerateRun(currentProgram, "Moderate", fitnessMetrics);
                }
                else
                {
                    fitnessMetrics = GenerateRun(currentProgram, "Easy", fitnessMetrics);
                }
            }
            fitnessMetrics = GenerateLift(currentProgram, fitnessMetrics);

            return new FitnessDictionary();
        }


        private FitnessDictionary GenerateRun(ClientProgram currentProgram, string runType, FitnessDictionary fitnessMetrics)
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

            fitnessMetrics.milePace = (double)currentProgram.MileMinutes + (Convert.ToDouble(currentProgram.MileSeconds) / 60);
            fitnessMetrics.milePace *= paceCoefficient;
            fitnessMetrics.runDuration = currentProgram.MinutesPerSession;
            
            //corner cases where runs share time with lifts in the same day.
            if (runType == "Easy")
            {
                fitnessMetrics.runDuration = currentProgram.MinutesPerSession / 2;
            }
            if (currentProgram.GoalCount == 1 && currentProgram.DaysPerWeek == 6 && fitnessMetrics.runType == "Easy")
            {
                fitnessMetrics.runDuration = currentProgram.MinutesPerSession;
            }
            if (runType == "Moderate" && currentProgram.DaysPerWeek < 4)
            {
                fitnessMetrics.runDuration = currentProgram.MinutesPerSession * .66;
            }

            fitnessMetrics.runType = "Easy";
            fitnessMetrics.distanceMiles = fitnessMetrics.runDuration / fitnessMetrics.milePace;
            fitnessMetrics = SharedUtility.ConvertFitnessDictCardioValues(fitnessMetrics);

            return fitnessMetrics;
        }
    }
}
