using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.Services
{
    public class ExerciseLibrary
    {
        [JsonProperty(PropertyName = "id")]
        public int ExerciseId { get; set; }

        [JsonProperty(PropertyName = "category")]
        public int BodyPart { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "name_original")]
        public string NameOriginal { get; set; }

        [JsonProperty(PropertyName = "muscles")]
        public int[] MusclesArray { get; set; }

        [JsonProperty(PropertyName = "muscles_secondary")]
        public int[] MusclesSecondary { get; set; }

        [JsonProperty(PropertyName = "equipment")]
        public object[] EquipmentRequired { get; set; }

        public string creation_date { get; set; }

        [JsonProperty(PropertyName = "language")]
        public int LanguageInteger { get; set; }
        public string uuid { get; set; }
        public object variations { get; set; }
    }
}
