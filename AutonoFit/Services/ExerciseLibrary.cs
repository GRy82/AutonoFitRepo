using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

    public class Exercise
    {
        public int id { get; set; }
        public string uuid { get; set; }
        public string name { get; set; }
        public string status { get; set; }
        public string description { get; set; }
        public string creation_date { get; set; }
        public int category { get; set; }
        public int?[] muscles { get; set; }
        public object[] muscles_secondary { get; set; }
        public int[] equipment { get; set; }
        public int language { get; set; }
        public int license { get; set; }
        public string license_author { get; set; }
        public int?[] variations { get; set; }
    }

}
