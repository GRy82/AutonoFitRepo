using AutonoFit.Contracts;
using AutonoFit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.StaticClasses
{
    public static class ProgramModule
    {
        private static readonly IRepositoryWrapper _repo;

        public static async Task<bool> ProgramNameTaken(string programName, int clientId)
        {
            List<ClientProgram> programs;
            try
            {
                programs = await _repo.ClientProgram.GetAllClientProgramsAsync(clientId);
            }
            catch(NullReferenceException)
            {
                programs = new List<ClientProgram> { };
            }

            foreach (ClientProgram program in programs)
            {
                if (program.ProgramName == programName)
                {
                    return true;
                }
            }

            return false;
        }

        public static async Task<int> GetWorkoutsCompletedByProgram(int programId)
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

        public static async Task<double> CalculateAttendanceRating(int programId, int workoutsCompleted)
        {
            int totalExpectedSessions = 0;
            ClientProgram clientProgram = await _repo.ClientProgram.GetClientProgramAsync(programId);
            TimeSpan timeSinceProgramStart = DateTime.Now - clientProgram.ProgramStart;
            totalExpectedSessions = workoutsCompleted / (clientProgram.DaysPerWeek * (timeSinceProgramStart.Days / 7));

            return totalExpectedSessions;
        }

    }
}
