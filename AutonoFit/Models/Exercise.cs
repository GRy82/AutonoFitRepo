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
            public int id { get; set; }
            //public string uuid { get; set; }

            [ForeignKey("Client")]
            public int ClientId { get; set; }
            public Client Client { get; set; }
            public string name { get; set; }

            [NotMapped]
            public string description { get; set; }
            //public string creation_date { get; set; }
            public int category { get; set; }

            [NotMapped]
            public int?[] muscles { get; set; }

            [NotMapped]
            public object[] muscles_secondary { get; set; }

            [NotMapped]
            public int[] equipment { get; set; }

            [NotMapped]
            public int language { get; set; }
            //public int license { get; set; }
            //public string license_author { get; set; }
            //public int?[] variations { get; set; }

            public int WorkoutId { get; set; }

            public int ProgramId { get; set; }
            public int RPE { get; set; }

            public int Reps { get; set; }

            public int Sets { get; set; }

            public int RestSeconds { get; set; }

            public string RestString { get; set; }

    }
}
