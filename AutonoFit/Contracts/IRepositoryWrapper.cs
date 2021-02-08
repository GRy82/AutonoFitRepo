using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.Contracts
{
    public interface IRepositoryWrapper
    {

        IClientRepository Client { get; }
        IClientEquipmentRepository ClientEquipment { get; }
        IEquipmentRepository Equipment { get; }
        IGoalsRepository Goals { get; }
        IClientExerciseRepository ClientExercise { get; }
        IClientWorkoutRepository ClientWorkout { get; }
        IClientProgramRepository ClientProgram { get; }
        IClientWeekRepository ClientWeek { get; }
    
        Task SaveAsync();
    }
}
