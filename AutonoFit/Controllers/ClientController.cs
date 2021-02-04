using AutonoFit.Contracts;
using AutonoFit.Models;
using AutonoFit.Services;
using AutonoFit.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AutonoFit.Controllers
{

    [Authorize(Roles = "Client")]
    public class ClientController : Controller
    {
        private IRepositoryWrapper _repo;
        private ExerciseLibraryService _exerciseLibraryService;
        public ClientController(IRepositoryWrapper repo, ExerciseLibraryService exerciseLibraryService)
        {
            _repo = repo;
            _exerciseLibraryService = exerciseLibraryService;
        }

        // GET: Client
        public async Task<ActionResult> Index()
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = await _repo.Client.GetClientAsync(userId);
          
            if(client == null)
            {
                return RedirectToAction("Create");
            }

            var equipmentList = _repo.ClientEquipment.GetClientEquipmentAsync(client.ClientId);

            if (equipmentList.Result.Count == 0)
            {
                return RedirectToAction("Equipment");
            }

            return View(client);
        }

        public async Task<ActionResult> AccountOverview()
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = await _repo.Client.GetClientAsync(userId);
            return View(client);
        }



        //---------------------------------------------------------------------------------------------------
        //-------------------------------------Single Workout------------------------------------------------



        public async Task<ActionResult> SingleWorkoutSetup(string errorMessage = null)
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = await _repo.Client.GetClientAsync(userId);


            SingleWorkoutVM singleWorkoutVM = new SingleWorkoutVM()
            {
                Client = client,
                AvailableGoals = await _repo.Goals.GetAllGoalsAsync(),
                GoalIds = new List<int> { 0, 0 },
                ErrorMessage = errorMessage
            };

            return View(singleWorkoutVM);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CheckSingleWorkoutFormValidity(SingleWorkoutVM singleWorkoutVM)
        {
            if(singleWorkoutVM.GoalIds[0] == 0 && singleWorkoutVM.GoalIds[1] == 0)//Client selected no goals.
            {
                return RedirectToAction("SingleWorkoutSetup", new RouteValueDictionary( new { controller = "Client", 
                    action = "SingleWorkoutSetup", errorMessage = "You must choose at least one exercise goal to continue." }));
            }
            if(singleWorkoutVM.BodySection == null)
            {
                return RedirectToAction("SingleWorkoutSetup", new RouteValueDictionary(new {controller = "Client", 
                    action = "SingleWorkoutSetup", errorMessage = "You must choose a workout type to continue."}));
            }

            return RedirectToAction("GatherEligibleExercises", singleWorkoutVM);
        }

       
        public async Task<ActionResult> GatherEligibleExercises(SingleWorkoutVM workoutVM)
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            workoutVM.Client = await _repo.Client.GetClientAsync(userId);
            workoutVM.Equipment = await _repo.ClientEquipment.GetClientEquipmentAsync(workoutVM.Client.ClientId);

            List<ExerciseLibrary> exerciseLibrary = new List<ExerciseLibrary> { };
            ExerciseLibrary singleExerciseLibrary;
            //Get exercises by category and repackage neatly.
            int[] categories = GetCategories(workoutVM.BodySection);
            for (int i = 0; i < categories.Length; i++)
            {
                string urlCategoryString = BuildEquipmentUrlString(workoutVM.Equipment) + "&category=" + categories[i];
                singleExerciseLibrary = await _exerciseLibraryService.GetExercises(urlCategoryString);
                exerciseLibrary = RepackageResults(exerciseLibrary, singleExerciseLibrary);
            }
            //Get exercises by muslces and repackage neatly.
            int[] muscles = GetMuscles(workoutVM.BodySection);
            string urlMusclesString = null;
            for (int j = 0; j < muscles.Length; j++)
            {
                urlMusclesString += "&muscles=" + muscles[j];
            }
            urlMusclesString = BuildEquipmentUrlString(workoutVM.Equipment) + urlMusclesString;
            singleExerciseLibrary = await _exerciseLibraryService.GetExercises(urlMusclesString);
            exerciseLibrary = RepackageResults(exerciseLibrary, singleExerciseLibrary);
            //Get rid of repeats
            exerciseLibrary = RemoveRepeats(exerciseLibrary);

            //Calculate sets/reps, rest time to exercises.
            Dictionary<string, int> SetsRepsRest = CalculateSetsRepsRest(workoutVM.GoalIds);

            //Decide number of exercises based on time constraints 

            //Randomly select N number of exercises from total collection thus far.  

            //Convert ExerciseLibrary objects to ClientExercises

            return RedirectToAction("DisplaySingleWorkout");      
        }



        public async Task<ActionResult> DisplaySingleWorkout(SingleWorkoutVM singleWorkoutVM)//this parameter subject to change. May be differe VM.
        {
            return RedirectToAction("Index");
        }

        //-----------------------------------Helper Methods----------------------------------------------------


        private List<ExerciseLibrary> RemoveRepeats(List<ExerciseLibrary> exerciseLibrary) {
            List<ExerciseLibrary> revisedLibrary = new List<ExerciseLibrary> { };
            foreach (ExerciseLibrary exercise in exerciseLibrary)
            {
                if (!revisedLibrary.Contains(exercise) && exercise.results[0].id != 393)//exercise 393 is trash. It's a full workout.
                {
                    revisedLibrary.Add(exercise);
                }
            }
            return revisedLibrary;
        }
        private List<ExerciseLibrary> RepackageResults(List<ExerciseLibrary> exerciseLibrary, ExerciseLibrary singleExerciseLibrary)
        {
            for (int i = 0; i < singleExerciseLibrary.results.Length; i++)
            {
                Result[] tempResult = new Result[1];
                tempResult[0] = singleExerciseLibrary.results[i];
                ExerciseLibrary tempExerciseLibrary = new ExerciseLibrary();
                tempExerciseLibrary.results = tempResult;
                exerciseLibrary.Add(tempExerciseLibrary);
            }

            return exerciseLibrary;
        }

        private string BuildEquipmentUrlString(List<ClientEquipment> equipmentList)
        {
            string urlString = "https://wger.de/api/v2/exercise?language=2";
            foreach (ClientEquipment piece in equipmentList){
                urlString += "&equipment=" + piece.EquipmentId;
            }

            return urlString;
        }

        private int[] GetCategories(string bodySection)
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
            return categories;
        }

        private int[] GetMuscles(string bodySection)
        {
            int[] muscles;
            switch (bodySection)
            {
                case "Upper Body":
                    muscles = new int[] { 1, 2, 3, 4, 5, 6, 9, 12, 13, 14};
                    break;
                case "Lower Body":
                    muscles = new int[] { 6, 7, 8, 10, 11, 14, 15 };
                    break;
                default:
                    muscles = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
                    break;
            }
            return muscles;
        }

        private Dictionary<string, int> CalculateSetsRepsRest(List<int> goalIds)
        {
            return new Dictionary<string, int> { };
        }


        //---------------------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------------------




        public async Task<ActionResult> Equipment()
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = await _repo.Client.GetClientAsync(userId);
            var clientEquipment = await _repo.ClientEquipment.GetClientEquipmentAsync(client.ClientId);
            var equipment = await _repo.Equipment.GetAllEquipmentAsync();

            ClientEquipmentVM clientEquipmentVM = GetClientEquipmentViewModel(client, clientEquipment, equipment);

            return View(clientEquipmentVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Equipment(ClientEquipmentVM clientEquipmentVM)
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = await _repo.Client.GetClientAsync(userId);
            var clientEquipment = await _repo.ClientEquipment.GetClientEquipmentAsync(client.ClientId);
            var equipment = await _repo.Equipment.GetAllEquipmentAsync();

            //clear past equipment
            foreach (ClientEquipment possession in clientEquipment)
            {
                _repo.ClientEquipment.DeleteClientEquipment(possession);
            }
            //add piece of equipment to ClientEquipment table for each piece checked/true.
            for(int i = 0; i < clientEquipmentVM.EquipmentChecks.Count; i++)
            {
                if(clientEquipmentVM.EquipmentChecks[i] == true)
                {
                    ClientEquipment addedEquipment = new ClientEquipment();
                    addedEquipment.EquipmentId = equipment[i].EquipmentId;
                    addedEquipment.ClientId = client.ClientId;
                    _repo.ClientEquipment.CreateClientEquipment(addedEquipment);
                }
            }
            await _repo.SaveAsync();

            return RedirectToAction("Index");
        }

        private ClientEquipmentVM GetClientEquipmentViewModel(Client client, List<ClientEquipment> clientEquipment, List<Equipment> equipment)
        {
            
            List<bool> equipmentChecks = new List<bool> { };
            for (int i = 0; i < equipment.Count; i++)
            {
                equipmentChecks.Add(false);
                foreach (ClientEquipment possessed in clientEquipment)
                {
                    if (possessed.EquipmentId == equipment[i].EquipmentId)
                    {
                        equipmentChecks[i] = true;
                        break;
                    }
                }
            }

            ClientEquipmentVM clientEquipmentVM = new ClientEquipmentVM()
            {
                Client = client,
                EquipmentList = equipment,
                EquipmentChecks = equipmentChecks
            };

            return clientEquipmentVM;
        }


        // GET: Client/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Client/Create
        public ActionResult Create()
        {
            return View(new Client());
        }

        // POST: Client/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Client client)
        {
            client.IdentityUserId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            _repo.Client.CreateClient(client);
            await _repo.SaveAsync();
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View("Create");
            }
        }

        // GET: Client/Edit/5
        public async Task<ActionResult> Edit()
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            Client client = await _repo.Client.GetClientAsync(userId);
            return View(client);
        }

        // POST: Client/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id)
        {
            Client client = await _repo.Client.GetClientAsync(id);
            _repo.Client.EditClient(client);
            await _repo.SaveAsync();

            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Client/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Client/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
