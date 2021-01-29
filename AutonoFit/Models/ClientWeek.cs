using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.Models
{
    public class ClientWeek
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Client")]
        public int ClientId { get; set; }
        public Client Client { get; set; }

        public DateTime WeekStart { get; set; }

        public DateTime WeekEnd { get; set; }

        public int WorkoutsExpected { get; set; }

        public int WorkoutsCompleted { get; set; }

        public bool Completed { get; set; }


        [ForeignKey("ClientWorkout")]
        public int? MostRecentWorkoutId { get; set; }
        public ClientWorkout ClientWorkout { get; set; }

        public int? LastWeekId { get; set; }
    }
}
