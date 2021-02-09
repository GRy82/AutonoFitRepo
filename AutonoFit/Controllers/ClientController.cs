using AutonoFit.Classes;
using AutonoFit.Contracts;
using AutonoFit.Models;
using AutonoFit.Services;
using AutonoFit.StaticClasses;
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
        private ProgramModule programModule;
        public ClientController(IRepositoryWrapper repo, ExerciseLibraryService exerciseLibraryService)
        {
            _repo = repo;
            programModule = new ProgramModule(_repo);
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
        public ActionResult CheckSingleWorkoutFormValidity(SingleWorkoutVM singleWorkoutVM)
        {
            if (singleWorkoutVM.GoalIds[0] == 0 && singleWorkoutVM.GoalIds[1] == 0)//Client selected no goals.
            {
                return RedirectToAction("SingleWorkoutSetup", new RouteValueDictionary(new
                {
                    controller = "Client",
                    action = "SingleWorkoutSetup",
                    errorMessage = "You must choose at least one exercise goal to continue."
                }));
            }
            if (singleWorkoutVM.BodySection == null)
            {
                return RedirectToAction("SingleWorkoutSetup", new RouteValueDictionary(new
                {
                    controller = "Client",
                    action = "SingleWorkoutSetup",
                    errorMessage = "You must choose a workout type to continue."
                }));
            }

            return RedirectToAction("GatherEligibleExercises", singleWorkoutVM);
        }


        public async Task<ActionResult> GatherEligibleExercises(SingleWorkoutVM workoutVM)
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            workoutVM.Client = await _repo.Client.GetClientAsync(userId);
            workoutVM.Equipment = await _repo.ClientEquipment.GetClientEquipmentAsync(workoutVM.Client.ClientId);

            List<Result> exerciseResults = new List<Result> { };
            ExerciseLibrary singleExerciseLibrary;
            //Get exercises by category and repackage neatly.
            int[] categories = SharedUtility.GetCategories(workoutVM.BodySection);
            for (int i = 0; i < categories.Length; i++)
            {
                string urlCategoryString = BuildEquipmentUrlString(workoutVM.Equipment) + "&category=" + categories[i];
                singleExerciseLibrary = await _exerciseLibraryService.GetExercises(urlCategoryString);
                exerciseResults = SharedUtility.RepackageResults(exerciseResults, singleExerciseLibrary);
            }
            //Get exercises by muslces and repackage neatly.
            int[] muscles = SharedUtility.GetMuscles(workoutVM.BodySection);
            string urlMusclesString = null;
            for (int j = 0; j < muscles.Length; j++)
            {
                urlMusclesString += "&muscles=" + muscles[j];
            }
            urlMusclesString = BuildEquipmentUrlString(workoutVM.Equipment) + urlMusclesString;
            singleExerciseLibrary = await _exerciseLibraryService.GetExercises(urlMusclesString);
            exerciseResults = SharedUtility.RepackageResults(exerciseResults, singleExerciseLibrary);
            //Get rid of repeats
            exerciseResults = SharedUtility.RemoveRepeats(exerciseResults);
            //Calculate sets/reps, rest time to exercises.
            FitnessDictionary fitnessMetrics = SingleWorkout.CalculateSetsRepsRest(workoutVM.GoalIds, workoutVM.Minutes, workoutVM.MileMinutes, workoutVM.MileSeconds);
            //if cardio is involved, cut minutes in half to have half the time for cardio.
            workoutVM.Minutes = fitnessMetrics.cardio == true ? (workoutVM.Minutes / 2) : workoutVM.Minutes ;
            //Decide number of exercises based on time constraints 
            int numberOfExercises = SharedUtility.DetermineVolume(fitnessMetrics, workoutVM.Minutes);
            //Randomly select N number of exercises from total collection thus far. 
            List<Result> randomlyChosenExercises = SharedUtility.RandomizeExercises(exerciseResults, numberOfExercises);
            //Convert ExerciseLibrary objects to ClientExercises
            List<ClientExercise> workoutExercises = SharedUtility.CopyAsClientExercises(randomlyChosenExercises, workoutVM, fitnessMetrics);
            workoutVM.fitnessDictionary = fitnessMetrics.cardio == true ? SharedUtility.ConvertFitnessDictCardioValues(fitnessMetrics) : fitnessMetrics;
            //Create new workout to contain exercises and other stored data.
            ClientWorkout workout = ClientWorkoutPseudoConstructor(workoutVM);
            _repo.ClientWorkout.CreateClientWorkout(workout);
            await _repo.SaveAsync();
            //assign all ClientExercises the workout Id
            await LoadExercisesInWorkout(workoutExercises, workout);
            workoutVM.Workout = workout;
            randomlyChosenExercises = CleanExerciseDescriptions(randomlyChosenExercises);
            //Place exercises in ViewModel
            workoutVM.Exercises = randomlyChosenExercises;
            //Run this regular garbage collection function for CLientExercises and ClientWorkouts that are more than a day old and not tied to a week/program.
            await CleanExerciseWorkoutDatabase();

            return View("DisplaySingleWorkout", workoutVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CompleteSingleWorkout(int workoutId)
        {
            ClientWorkout clientWorkout = await _repo.ClientWorkout.GetClientWorkoutAsync(workoutId);
            clientWorkout.Completed = true;
            _repo.ClientWorkout.EditClientWorkout(clientWorkout);
            await _repo.SaveAsync();

            return RedirectToAction("Index");
        }

        //---------------------------------------------------------------------------------------------------
        //-------------------------------------Program Workouts----------------------------------------------

        public async Task<ActionResult> ProgramSetup(string errorMessage = null)
        {
            ProgramSetupVM programSetupVM = new ProgramSetupVM()
            {
                AvailableGoals = await _repo.Goals.GetAllGoalsAsync(),
                GoalIds = new List<int> { 0, 0 },
                ErrorMessage = errorMessage
            };

            return View(programSetupVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CheckProgramFormValidity(ProgramSetupVM programSetuptVM)
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = await _repo.Client.GetClientAsync(userId);

            if (programSetuptVM.GoalIds[0] == 0 && programSetuptVM.GoalIds[1] == 0)//Client selected no goals.
            {
                return RedirectToAction("ProgramSetup", new RouteValueDictionary(new { controller = "Client", action = "ProgramSetup",
                    errorMessage = "You must choose at least one exercise goal to continue." }));
            }

            if (await programModule.ProgramNameTaken(programSetuptVM.ProgramName, client.ClientId))
            {
                return RedirectToAction("ProgramSetup", new RouteValueDictionary(new { controller = "Client", action = "ProgramSetup",
                    errorMessage = "That name is already taken." }));
            }

            ClientProgram clientProgram = new ClientProgram() {
                ProgramName = programSetuptVM.ProgramName,
                ClientId = client.ClientId,
                GoalCount = programSetuptVM.GoalIds.Count,
                GoalOneId = programSetuptVM.GoalIds[0],
                GoalTwoId = programSetuptVM.GoalIds[1], //Will be 0 if not set to a goal. Maybe change to a null conditional in the future.
                MinutesPerSession = programSetuptVM.Minutes,
                DaysPerWeek = programSetuptVM.Days,
                MileMinutes = programSetuptVM?.MileMinutes,
                MileSeconds = programSetuptVM?.MileSeconds,
                ProgramStart = DateTime.Now,
            };

            _repo.ClientProgram.CreateClientProgram(clientProgram);
            await _repo.SaveAsync();

            return RedirectToAction("ProgramOverview", new { programId = clientProgram.ProgramId });
        }

        public async Task<ActionResult> ProgramsList(int clientId)
        {
            List<ClientProgram> programs = await _repo.ClientProgram.GetAllClientProgramsAsync(clientId);

            return View(programs);
        }


        public async Task<ActionResult> ProgramOverview(int programId)
        {
            ClientProgram clientProgram = await _repo.ClientProgram.GetClientProgramAsync(programId);
            Client client = await _repo.Client.GetClientAsync(clientProgram.ClientId);
            int workoutsCompleted = await programModule.GetWorkoutsCompletedByProgram(programId);
            string goalTwoName = clientProgram.GoalTwoId == 0 ? null : (await _repo.Goals.GetGoalsAsync(Convert.ToInt32(clientProgram.GoalTwoId))).Name;
            ProgramOverviewVM programOverviewVM = new ProgramOverviewVM()
            {
                WorkoutsCompleted = workoutsCompleted,
                ClientProgram = clientProgram,
                ClientName = client.FirstName + " " + client.LastName,
                GoalOneName = (await _repo.Goals.GetGoalsAsync(clientProgram.GoalOneId)).Name,
                GoalTwoName = goalTwoName,
                ProgramStart = clientProgram.ProgramStart.Date.ToString("MM/dd/yy"),
                AttendanceRating = await programModule.CalculateAttendanceRating(programId, workoutsCompleted)
            };

            return View(programOverviewVM);
        }

        public async Task<ActionResult> GenerateProgramWorkout(int programId)
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            Client client = await _repo.Client.GetClientAsync(userId);
            ClientProgram currentProgram = await _repo.ClientProgram.GetClientProgramAsync(programId);
            List<ClientWorkout> recentWorkoutCycle = await GatherWorkoutCycle(currentProgram);
            FitnessDictionary fitnessMetrics = new FitnessDictionary();
            List<int> goalIds = new List<int> { currentProgram.GoalOneId, Convert.ToInt32(currentProgram.GoalTwoId) };
            fitnessMetrics.cardio = goalIds.Contains(4) || goalIds.Contains(5) ? true : false;

            int todaysGoalNumber = programModule.GetTodaysGoal(recentWorkoutCycle, goalIds, currentProgram.GoalCount);
            if (todaysGoalNumber == 4 || todaysGoalNumber == 5) {
                
            }
            else
            {
                string bodyParts = programModule.GetBodyParts(recentWorkoutCycle, todaysGoalNumber, currentProgram.GoalCount);
                
            }


            if (currentProgram.GoalCount == 1)//don't alternate goals, there's only one.
            {
                if (fitnessMetrics.cardio)//it is cardio. alternate the distance 
                {
                    fitnessMetrics = await programModule.GetTodaysCardio(fitnessMetrics, recentWorkoutCycle, todaysGoalNumber, currentProgram);
                }
                else // not cardio, just alternate lower/upperbody, //then alternate sets/reps
                {
                    string bodyParts = programModule.GetBodyParts(recentWorkoutCycle, todaysGoalNumber, currentProgram.GoalCount);
                    fitnessMetrics = programModule.GenerateLift(currentProgram, recentWorkoutCycle, fitnessMetrics, todaysGoalNumber);
                }
            }
            else//alternate goals for workout
            {
                if (fitnessMetrics.cardio && todaysGoalNumber == 4 || todaysGoalNumber == 5) //If today is a cardio day.
                {
                    fitnessMetrics = await programModule.GetTodaysCardio(fitnessMetrics, recentWorkoutCycle, todaysGoalNumber, currentProgram);
                }
                else//not cardio, alternate upper/lower body, //then alternate sets/reps
                {
                    string bodyParts = programModule.GetBodyParts(recentWorkoutCycle, todaysGoalNumber, currentProgram.GoalCount);
                    fitnessMetrics = programModule.GenerateLift(currentProgram, recentWorkoutCycle, fitnessMetrics, todaysGoalNumber);
                }
            }
            int numberOfExercises = SharedUtility.DetermineVolume(fitnessMetrics, currentProgram.MinutesPerSession);
           
            ProgramWorkoutVM programWorkoutVM = new ProgramWorkoutVM()
            {
            };

            //1. Consider days/week. Consider 2 aspects about them 1.) even/odd 2.) actual number. 2 is the minumum, 6 is the max.

            //2. Consider Number of goals.  1 goal: Cardio? yes => alternate cardio and lifting.  No => between lower reps/higher sets and higher reps/lower sets.
            //                              2 goals: Cardio? yes => alternate cardio and lifting.

            return View("DisplayProgramWorkout", programWorkoutVM);
        }

        //-------------------------------------------------------------------------------------------------------
        //-----------------------------------Helper Methods----------------------------------------------------

        public async Task<List<ClientWorkout>> GatherWorkoutCycle(ClientProgram currentProgram)
        {
            List<ClientWorkout> recentWorkouts = await _repo.ClientWorkout.GetAllClientWorkoutsAsync(currentProgram.ClientId);
            recentWorkouts = (List<ClientWorkout>)recentWorkouts.OrderByDescending(c => c.DatePerformed);
            List<ClientWorkout> lastWorkoutCycle = new List<ClientWorkout>() { };
            for(int i = 0; i < currentProgram.DaysPerWeek; i++) 
            {
                lastWorkoutCycle.Add(recentWorkouts[i]);
            }

            return lastWorkoutCycle;
        }


        private async Task CleanExerciseWorkoutDatabase()
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            Client client = await _repo.Client.GetClientAsync(userId);

            List<ClientWorkout> oldWorkouts = await _repo.ClientWorkout.GetOldWorkoutsAsync(client.ClientId);
            foreach (ClientWorkout workout in oldWorkouts)
            {
                List<ClientExercise> oldExercises = await _repo.ClientExercise.GetClientExerciseByWorkoutAsync(workout.Id);
                foreach (ClientExercise exercise in oldExercises)
                {
                    _repo.ClientExercise.DeleteClientExercise(exercise);
                }
                _repo.ClientWorkout.DeleteClientWorkout(workout);
            }

            await _repo.SaveAsync();
        }

        private string BuildEquipmentUrlString(List<ClientEquipment> equipmentList)
        {
            string urlString = "https://wger.de/api/v2/exercise?language=2&equipment=7";
            foreach (ClientEquipment piece in equipmentList){
                urlString += "&equipment=" + piece.EquipmentId;
            }

            return urlString;
        }

        public ClientWorkout ClientWorkoutPseudoConstructor(SingleWorkoutVM workoutVM)
        {
            ClientWorkout workout = new ClientWorkout();
            workout.ClientId = workoutVM.Client.ClientId;
            workout.mileDistance = workoutVM.fitnessDictionary.distanceMiles;
            workout.milePaceSeconds = (int)(workoutVM.fitnessDictionary.milePace * 60);
            workout.DatePerformed = DateTime.Now;
            return workout;
        }

        public async Task LoadExercisesInWorkout(List<ClientExercise> workoutExercises, ClientWorkout workout)
        {
            foreach (ClientExercise exercise in workoutExercises)
            {
                exercise.WorkoutId = workout.Id;
                _repo.ClientExercise.CreateClientExercise(exercise);
            }
            await _repo.SaveAsync();
        }

        private List<Result> CleanExerciseDescriptions(List<Result> exercises)
        {
            foreach (Result exercise in exercises)
            {
                exercise.description = SharedUtility.RemoveTags(exercise.description);
            }

            return exercises;
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


        // POST: Client/Delete Program/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteProgram(int id)
        {
            try
            {
                ClientProgram clientProgram = await _repo.ClientProgram.GetClientProgramAsync(id);
                List<ClientWeek> clientWeeks = await _repo.ClientWeek.GetAllClientWeeksAsync(clientProgram.ProgramId); 
                foreach (ClientWeek week in clientWeeks)
                {
                    List<ClientWorkout> clientWorkouts = await _repo.ClientWorkout.GetAllWorkoutsByWeekAsync(week.Id);
                    foreach (ClientWorkout workout in clientWorkouts)
                    {
                        List<ClientExercise> clientExercises = await _repo.ClientExercise.GetClientExerciseByWorkoutAsync(workout.Id); 
                        foreach (ClientExercise exercise in clientExercises)
                        {
                            _repo.ClientExercise.DeleteClientExercise(exercise);
                        }
                        _repo.ClientWorkout.DeleteClientWorkout(workout);
                    }
                    _repo.ClientWeek.DeleteClientWeek(week);
                }
                _repo.ClientProgram.DeleteClientProgram(clientProgram);
                await _repo.SaveAsync();

                return RedirectToAction("ProgramsList");
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }
    }
}
