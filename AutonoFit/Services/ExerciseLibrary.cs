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
        public string next { get; set; }
        public object previous { get; set; }
        public Result[] results { get; set; }
    }

    public class Result
    {
        public int id { get; set; }
        public int category { get; set; }
        public string description { get; set; }
        public string name { get; set; }
        public string name_original { get; set; }
        public int?[] muscles { get; set; }
        public int?[] muscles_secondary { get; set; }
        public int?[] equipment { get; set; }
        public string creation_date { get; set; }
        public int language { get; set; }
        public string uuid { get; set; }
        public int? variations { get; set; }
    }

}
