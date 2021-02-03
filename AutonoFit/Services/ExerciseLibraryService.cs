using AutonoFit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AutonoFit.Services
{
    public class ExerciseLibraryService
    {
        public ExerciseLibraryService()
        {

        }

        public async Task<ExerciseLibrary> GetExercisesByEquipment(Equipment equipment)
        {
            
            
        }

        public async Task<ExerciseLibrary> GetOneExerciseByEquipment()
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync($"https://wger.de/api/v2/exercise?language=2&equipment={equipment.EquipmentId}");

        }

    }
}
