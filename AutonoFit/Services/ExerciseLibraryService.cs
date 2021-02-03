using AutonoFit.Models;
using AutonoFit.ViewModels;
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

        public List<ExerciseLibrary> GetExercises(SingleWorkoutVM singleWorkoutVM)
        {

            List<ExerciseLibrary> possibleExercises = new List<ExerciseLibrary> { };

            //Attain full list of exercises eligible based on lang=2, equipment, category, and muscles

            //Search by category and equipment, one category at a time.

            //Search by muscles and equipment, one piece of equipment at a time.

            //Remove repeat exercises by ID repetition


            //category is json key. BodyPart is the ExerciseLibrary equivalent
            List<ExerciseLibrary> exercisesByEquipment = GetExercisesByCategory(singleWorkoutVM.Equipment, singleWorkoutVM.BodySection);
        

            return possibleExercises;
            
        }

        public async Task<List<ExerciseLibrary>> GetExercisesByCategory(List<ClientEquipment> equipmentList, string bodySection)
        {
            int[] categories;
            switch (bodySection)
            {
                case "Upper Body":
                    categories = new int[] { 8, 10, 11, 12, 13 };
                    break;
                case "Lower Body":
                    categories = new int[] { 9, 10, 14 };
                    break;
                default:
                    categories = new int[] { 8, 9, 10, 11, 12, 13, 14 };
                    break;
            }

            string equipmentString = BuildEquipmentUrlString(equipmentList);

            HttpClient client = new HttpClient();

            for (int i = 0; i < categories.Length; i++)
            {
                
                HttpResponseMessage response = await client.GetAsync($"https://wger.de/api/v2/exercise?language=2&category={categories[i]}{equipmentString}" );
            }
            

        }

        private string BuildEquipmentUrlString(List<ClientEquipment> equipmentList)
        {
                string equipmentString = null;
                foreach (ClientEquipment piece in equipmentList)
                {
                    equipmentString += "&equipment=" + piece.EquipmentId;
                }

                return equipmentString;
        }

    }

}
