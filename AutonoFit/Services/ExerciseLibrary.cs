using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutonoFit.Models;

namespace AutonoFit.Services
{
    public class ExerciseLibrary
    {
        public int count { get; set; }
        public object next { get; set; }
        public object previous { get; set; }
        
        [JsonProperty(PropertyName = "results")]
        public Exercise[] exercises { get; set; }
    }

    

}
