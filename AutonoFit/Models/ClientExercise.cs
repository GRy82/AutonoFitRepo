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

        public int WorkoutId { get; set; }
        public int RPE { get; set; }

        public int NumberRM { get; set;} //Rep-MAx, ie. 3-RM, 10-RM, etc.

        public int Reps { get; set; }

        public int Sets { get; set; }

        public int RestSeconds { get; set; }

        //These are used for gauging progression

        public string LastAdjusted { get; set; } //could be sets, reps, or rest. Alternate which is progressed.
        public int DeltaRPECount { get; set; }

        public int? LastPerformed { get; set; } //Id of the exercise the last time it was performed

        public TimeSpan? TimeSinceLast { get; set; } //Time since this exercise(a ClientExercise with same ExerciseId) was last performed.
    }
}
