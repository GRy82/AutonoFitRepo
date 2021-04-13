using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.Models
{
    public class Exercise
    {
            [Key]
            public int Id { get; set; }

            [JsonProperty(PropertyName = "id")]
            public int exerciseId { get; set; }

            [ForeignKey("Client")]
            public int ClientId { get; set; }
            public Client Client { get; set; }
            public string name { get; set; }

            [NotMapped]
            public string description { get; set; }

            public int WorkoutId { get; set; }

            public int ProgramId { get; set; }

            public int RPE { get; set; }

            public int Reps { get; set; }

            public int Sets { get; set; }

            public int RestSeconds { get; set; }

            public string RestString { get; set; }

    }
}
