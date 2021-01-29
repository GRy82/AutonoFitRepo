using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.Models
{
    public class Client
    {
        [Key]
        public int ClientId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int BirthMonth { get; set; }

        public int BirthDay { get; set; }

        public int BirthYear { get; set; }

        public int Age { get; set; }

    }
}
