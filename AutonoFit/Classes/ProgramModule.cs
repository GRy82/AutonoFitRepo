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

    }
}
