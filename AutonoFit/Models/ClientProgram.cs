using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.Models
{
    public class ClientProgram
    {
        [Key]
        public int ProgramId { get; set; }

        [ForeignKey("Client")]
        public int ClientId { get; set; }
        public Client Client { get; set; }

        public int MinutesPerSession { get; set; }

        public int DaysPerWeek { get; set; }

        public string ProgramName { get; set; }

        public int GoalOneId { get; set; }

        public int? GoalTwoId { get; set; }

        public DateTime ProgramStart { get; set; }

    }
}
