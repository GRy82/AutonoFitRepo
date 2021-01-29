using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.Models
{
    public class PeriodGoals
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Goals")]
        public int GoalId { get; set; }
        public Goals Goals { get; set; }
       

        [ForeignKey("ClientWeek")]
        public int WeekId { get; set; }

        [ForeignKey("ClientWorkout")]
        public int WorkoutId { get; set; }
        public ClientWorkout ClientWorkout { get; set; }



    }
}
