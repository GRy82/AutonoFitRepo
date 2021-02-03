using AutonoFit.Models;
using AutonoFit.ViewModels;
using Newtonsoft.Json;
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

        public async Task<ExerciseLibrary> GetExercises(string urlString)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(urlString);

            if (response.IsSuccessStatusCode)
            {
                string json = response.Content.ReadAsStringAsync().Result;
                var donk = JsonConvert.DeserializeObject<ExerciseLibrary>(json);
                return donk;
            }

            return null;
        }

    }

}
