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

        [ForeignKey("ClientId")]
        public int ClientId { get; set; }
        public Client Client { get; set; }

        public int? ProgramId { get; set; }

        public string BodyParts { get; set; } //"Upper Body" or "Lower Body"

        public int? GoalId { get; set; }

        public string RunType { get; set; }

        public int? milePaceSeconds { get; set; }

        public double? mileDistance { get; set; }

        public bool Completed { get; set; }

        public DateTime? DatePerformed { get; set; }

        public int? CardioRPE { get; set; }

    }
}
