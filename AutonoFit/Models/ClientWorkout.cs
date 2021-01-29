﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.Models
{
    public class ClientWorkout
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("ClientWeek")]
        public int? WeekId { get; set; }
        public ClientWeek ClientWeek { get; set; }

        public bool Completed { get; set; }

        public DateTime? DatePerformed { get; set; }

        public int? OverallDifficultyRating { get; set; }

        public int? LastWorkoutId { get; set; }





    }
}
