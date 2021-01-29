using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.Models
{
    public class ClientExercise
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Client")]
        public int ClientId { get; set; }
        public Client Client { get; set; }

        public int ExerciseId { get; set; }
        public int? WeekId { get; set; }
        public int WorkoutId { get; set; }
        public int RPE { get; set; }

        public int Reps { get; set; }
        public int DeltaRPECount { get; set; }

        public int? LastPerformed { get; set; } //Id of the exercise the last time it was performed

        public TimeSpan TimeSinceLast { get; set; } //Time since this exercise(a ClientExercise with same ExerciseId) was last performed.
    }
}
