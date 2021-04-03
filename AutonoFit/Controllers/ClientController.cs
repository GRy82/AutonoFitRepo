using AutonoFit.Classes;
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
        private ProgramModule programModule;
        private SingleModule singleModule;
        public ClientController(IRepositoryWrapper repo, ExerciseLibraryService exerciseLibraryService)
        {
            _repo = repo;
            _exerciseLibraryService = exerciseLibraryService;
            programModule = new ProgramModule(_repo);
            singleModule = new SingleModule(exerciseLibraryService);
        }

        private string GetUserId()
        {
            return this.User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        // GET: Client
        public async Task<ActionResult> Index(bool accountNewlyCreated = false)
        {
            var client = await _repo.Client.GetClientAsync(GetUserId());
          
            if(client == null)
            {
                return RedirectToAction("Create");
            }

            var equipmentList = _repo.ClientEquipment.GetClientEquipmentAsync(client.ClientId);

            if (accountNewlyCreated == true)
            {
                return RedirectToAction("Equipment");
            }

            return View(client);
        }


        public async Task<ActionResult> AccountOverview()
        {
            var client = await _repo.Client.GetClientAsync(GetUserId());
            return View(client);
        }



        //---------------------------------------------------------------------------------------------------
        //-------------------------------------Single Workout------------------------------------------------

        public async Task<ActionResult> SingleWorkoutSetup(string errorMessage = null)
        {
            var client = await _repo.Client.GetClientAsync(GetUserId());
            SingleWorkoutVM singleWorkoutVM = await InstantiateSingleWorkoutVM(client, errorMessage);
         
            return View(singleWorkoutVM);
        }

        public async Task<SingleWorkoutVM> InstantiateSingleWorkoutVM(Client client, string errorMessage = null)
        {
            return new SingleWorkoutVM()
            {
                Client = client,
                AvailableGoals = await _repo.Goals.GetAllGoalsAsync(),
                GoalIds = new List<int> { 0, 0 },
                ErrorMessage = errorMessage,
                DiscourageHighIntensity = await RecommendAgainstHighIntensity()
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckSingleWorkoutFormValidity(SingleWorkoutVM singleWorkoutVM)
        {
            string errorMessage = null;
            bool noGoalsSelected = singleWorkoutVM.GoalIds[0] == 0 && singleWorkoutVM.GoalIds[1] == 0;
            bool noBodySectionSelected = singleWorkoutVM.BodySection == null;

            if (noGoalsSelected)
                errorMessage = "You must choose at least one exercise goal to continue.";
            
            if (noBodySectionSelected)
                errorMessage = "You must choose a workout type to continue.";
            
            if (noGoalsSelected || noBodySectionSelected) {
                return RedirectToAction("SingleWorkoutSetup", new RouteValueDictionary(new
                {
                    controller = "Client",
                    action = "SingleWorkoutSetup",
                    errorMessage = errorMessage
                }));
            }

            return RedirectToAction("ConstructWorkoutVM", singleWorkoutVM);
        }


        public async Task<ActionResult> ConstructWorkoutVM(SingleWorkoutVM workoutVM)
        {
            workoutVM.Client = await _repo.Client.GetClientAsync(GetUserId());
            workoutVM.Equipment = await _repo.ClientEquipment.GetClientEquipmentAsync(workoutVM.Client.ClientId);
            List<Exercise> exercises = await GatherExercises(workoutVM);
            FitnessParameters fitnessParameters = new FitnessParameters();
            fitnessParameters.SetFitnessParameters(workoutVM); //Calculate sets/reps, rest time to exercises.
            workoutVM.Minutes = fitnessParameters.cardioComponent != null ? (workoutVM.Minutes / 2) : workoutVM.Minutes; //if cardio is involved, cut minutes in half to have half the time for cardio.
            workoutVM.FitnessParameters = fitnessParameters;
            int numberOfExercises = SharedUtility.GetExerciseQty(fitnessParameters, workoutVM.Minutes); 
            List<Exercise> randomlyChosenExercises = SharedUtility.RandomizeExercises(exercises, numberOfExercises);
            ClientWorkout workout = InstantiateClientWorkout(workoutVM); //Create new workout to contain exercises and other stored data.
            workoutVM.Workout = workout; //assign all ClientExercises the workout Id
            randomlyChosenExercises = CleanseExerciseDescriptions(randomlyChosenExercises);
            workoutVM.Exercises = randomlyChosenExercises;  //Place exercises in ViewModel

            return View("DisplaySingleWorkout", workoutVM);
        }

        private async Task<List<Exercise>> GatherExercises(SingleWorkoutVM workoutVM)
        {
            List<Exercise> exercises = await singleModule.FindExercisesByCategory(workoutVM, new List<Exercise> { }); //Get exercises by category and repackage into Result reference type.
            exercises = await singleModule.FindExercisesByMuscles(workoutVM, exercises); //Get exercises by muslces and repackage into Result reference type.
            exercises = SharedUtility.RemoveRepeats(exercises); //Get rid of repeats
            return exercises;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CompleteSingleWorkout()
        {
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
                ErrorMessage = errorMessage,
                DiscourageHighIntensity = await RecommendAgainstHighIntensity()
            };

            return View(programSetupVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CheckProgramFormValidity(ProgramSetupVM programSetupVM)
        {
            var client = await _repo.Client.GetClientAsync(GetUserId());

            if(await NotValidProgramSetup(programSetupVM, client))
            {
                return RedirectToAction("ProgramSetup", new RouteValueDictionary(new
                {
                    controller = "ClientController",
                    action = "ProgramSetup",
                    errorMessage = GetProgramSetupErrorMessage(programSetupVM, client)
                }));
            }

            ClientProgram clientProgram = InstantiateClientProgram(programSetupVM, client);
            _repo.ClientProgram.CreateClientProgram(clientProgram);
            await _repo.SaveAsync();

            return RedirectToAction("ProgramOverview", new { programId = clientProgram.ProgramId });
        }

        private async Task<bool> NotValidProgramSetup(ProgramSetupVM programSetupVM, Client client)
        {
            return programSetupVM.GoalIds[0] == 0
                || await programModule.ProgramNameTaken(programSetupVM.ProgramName, client.ClientId)
                || programSetupVM.GoalIds.Contains(4) && programSetupVM.GoalIds.Contains(5);
        }

        private async Task<string> GetProgramSetupErrorMessage(ProgramSetupVM programSetupVM, Client client)
        {
            string errorMessage = null;
            if (programSetupVM.GoalIds[0] == 0 && programSetupVM.GoalIds[1] == 0)//Client selected no goals.
                errorMessage = "You must choose at least one exercise goal to continue.";
            if (await programModule.ProgramNameTaken(programSetupVM.ProgramName, client.ClientId))
                errorMessage = "That name is already taken.";
            if (programSetupVM.GoalIds[0] == 0)
                errorMessage = "Select the first goal if you only choose one.";
            if (programSetupVM.GoalIds.Contains(4) && programSetupVM.GoalIds.Contains(5))
                errorMessage = "You can only choose one cardio-intensive goal due to high overuse injury risk.";

            return errorMessage;
        }

        private ClientProgram InstantiateClientProgram(ProgramSetupVM programSetupVM, Client client)
        {
            return new ClientProgram()
            {
                ProgramName = programSetupVM.ProgramName,
                ClientId = client.ClientId,
                GoalOneId = programSetupVM.GoalIds[0],
                GoalTwoId = programSetupVM.GoalIds[1],
                GoalCount = programSetupVM.GoalIds[1] == 0 ? 1 : 2,
                MinutesPerSession = programSetupVM.Minutes,
                DaysPerWeek = programSetupVM.Days,
                MileMinutes = programSetupVM?.MileMinutes,
                MileSeconds = programSetupVM?.MileSeconds,
                ProgramStart = DateTime.Now,
            };
        }

        public async Task<ActionResult> ProgramsList(int clientId)
        {
            Client client = await _repo.Client.GetClientAsync(GetUserId());
            List<ClientProgram> programs = await _repo.ClientProgram.GetAllClientProgramsAsync(client.ClientId);

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

        private async Task<List<Exercise>> GetExercisesByEquipment(Client client)
        {
            List<ClientEquipment> equipment = await _repo.ClientEquipment.GetClientEquipmentAsync(client.ClientId);
            string url = SharedUtility.BuildEquipmentUrlString(equipment);
            ExerciseLibrary exerciseLibrary = await _exerciseLibraryService.GetExercises(url);
            return SharedUtility.AddLibrarytoExercises(new List<Exercise>(), exerciseLibrary);
        }

        public async Task<ActionResult> GenerateProgramWorkout(int programId)
        {
            Client client = await _repo.Client.GetClientAsync(GetUserId());
            List<Exercise> resultsLibrary = await GetExercisesByEquipment(client);

            List<Exercise> todaysExercises = new List<Exercise> { };
            ClientProgram currentProgram = await _repo.ClientProgram.GetClientProgramAsync(programId);
            List<FitnessParameters> fitnessParameters = new List<FitnessParameters> { };
            List<ClientWorkout> recentWorkoutCycle = await GatherWorkoutCycle(currentProgram);
            int todaysGoalNumber = programModule.GetTodaysGoal(recentWorkoutCycle, currentProgram);
            
            if (todaysGoalNumber == 4 || todaysGoalNumber == 5) {//if cardio in any capactiy
                fitnessParameters.Add(await programModule.GetTodaysCardio(new FitnessParameters(), recentWorkoutCycle, todaysGoalNumber, currentProgram));
            }
            string bodyParts = null;
            List<ClientExercise> clientExercises = new List<ClientExercise> { };
            if (todaysGoalNumber != 4 && todaysGoalNumber != 5 || (fitnessParameters.Count != 0 && (fitnessParameters[0].runType == "Easy" || fitnessMetrics[0].runType == "6-Lift")))//Generate a Lifting componenent
            {
                int liftLengthMinutes = 0;
                int totalExerciseTime = 0;
                if (todaysGoalNumber != 4 && todaysGoalNumber != 5)// if it is a purely liftng workout
                {
                    bodyParts = programModule.GetBodyParts(recentWorkoutCycle, todaysGoalNumber, currentProgram.GoalCount);
                    liftLengthMinutes = currentProgram.MinutesPerSession;
                }
                else//if supplemental cardio is needed or a lift for someone who does too much cardio("6 Lift")
                {
                    bodyParts = fitnessParameters[0].runType == "Easy" ? "Both" : "Upper Body";//full-body lift w/ easy run OR solely upper-body exercise
                    liftLengthMinutes = fitnessParameters[0].runType == "Easy" ? Convert.ToInt32(fitnessParameters[0].runDuration) : currentProgram.MinutesPerSession;
                }
                while (totalExerciseTime < liftLengthMinutes)
                {
                    Exercise exercise = SharedUtility.SelectExercise(bodyParts, resultsLibrary, todaysExercises);
                    exercise.description = SharedUtility.RemoveTags(exercise.description);
                    todaysExercises.Add(exercise);
                    FitnessParameters tempFitDict = new FitnessParameters();
                    tempFitDict = await programModule.GenerateLift(currentProgram, recentWorkoutCycle, tempFitDict, todaysGoalNumber, exercise.id);
                    fitnessParameters.Add(tempFitDict);
                    ClientExercise clienteExercise = SharedUtility.CopyAsClientExercises(exercise, client.ClientId, tempFitDict);
                    clientExercises.Add(clienteExercise);
                    totalExerciseTime += (int)(SharedUtility.GetSingleExerciseTime(tempFitDict) / 60);
                } 
            }

            ClientWorkout clientWorkout = InstantiateClientWorkout(fitnessParameters, client, bodyParts, currentProgram, todaysGoalNumber);
            _repo.ClientWorkout.CreateClientWorkout(clientWorkout);
            AddClientExercises(clientExercises, currentProgram, clientWorkout);
            await _repo.SaveAsync();

            ProgramWorkoutVM programWorkoutVM = InstantiateProgramWorkoutVM(clientExercises, todaysExercises, fitnessParameters, clientWorkout);

            return View("DisplayProgramWorkout", programWorkoutVM);
        }

        private void AddClientExercises(List<ClientExercise> clientExercises, ClientProgram currentProgram, ClientWorkout clientWorkout)
        {
            foreach (ClientExercise exercise in clientExercises)
            {
                exercise.WorkoutId = clientWorkout.Id;
                exercise.ProgramId = currentProgram.ProgramId;
                _repo.ClientExercise.CreateClientExercise(exercise);
            }
        }

        private ClientWorkout InstantiateClientWorkout(List<FitnessParameters> fitnessMetrics, Client client, string bodyParts, ClientProgram currentProgram, int todaysGoalNumber)
        {
            return new ClientWorkout()
            {
                ClientId = client.ClientId,
                ProgramId = currentProgram.ProgramId,
                BodyParts = bodyParts,
                GoalId = todaysGoalNumber,
                RunType = fitnessMetrics[0].runType,
                milePaceSeconds = Convert.ToInt32(fitnessMetrics[0].milePace * 60),
                mileDistance = fitnessMetrics[0].distanceMiles,
                DatePerformed = DateTime.Now
            };
        }

        private ProgramWorkoutVM InstantiateProgramWorkoutVM(List<ClientExercise> clientExercises, List<Exercise> todaysExercises, List<FitnessParameters> fitnessMetrics, ClientWorkout clientWorkout)
        {
            return new ProgramWorkoutVM()
            {
                ClientExercises = clientExercises,
                Exercises = todaysExercises,
                FitnessDictionary = fitnessMetrics,
                ClientWorkout = clientWorkout
            };
        }

        public async Task<ActionResult> CompleteProgramWorkout(ProgramWorkoutVM programWorkoutVM)
        {
            ClientWorkout clientWorkout = await _repo.ClientWorkout.GetClientWorkoutAsync(programWorkoutVM.ClientWorkout.Id);
            clientWorkout.CardioRPE = programWorkoutVM.CardioRPE;
            clientWorkout.Completed = true;
            _repo.ClientWorkout.EditClientWorkout(clientWorkout);
            List<ClientExercise> clientExercises = await _repo.ClientExercise.GetClientExerciseByWorkoutAsync(clientWorkout.Id);
            for (int i = 0; i < clientExercises.Count; i++)
            {
                clientExercises[i].RPE = programWorkoutVM.RPEs[i];
                _repo.ClientExercise.EditClientExercise(clientExercises[i]);
            }
            await _repo.SaveAsync();

            return RedirectToAction("ProgramsList");
        }

        //-------------------------------------------------------------------------------------------------------
        //-----------------------------------Helper Methods----------------------------------------------------



        public async Task<List<ClientWorkout>> GatherWorkoutCycle(ClientProgram currentProgram)
        {
            List<ClientWorkout> recentWorkouts = await _repo.ClientWorkout.GetAllWorkoutsByProgramAsync(currentProgram.ProgramId);
            if (recentWorkouts.Count == 0) {
                return new List<ClientWorkout> { };
            }

            var workouts = from s in recentWorkouts
                                orderby s.Id descending
                                select s;


            List<ClientWorkout> lastWorkoutCycle = new List<ClientWorkout> { };
            foreach (var workout in workouts)
            {
                ClientWorkout convertedWorkout = new ClientWorkout();
                convertedWorkout.Id = workout.Id;
                convertedWorkout.CardioRPE = workout.CardioRPE;
                convertedWorkout.BodyParts = workout.BodyParts;
                convertedWorkout.ClientId = workout.ClientId;
                convertedWorkout.Completed = workout.Completed;
                convertedWorkout.DatePerformed = workout.DatePerformed;
                convertedWorkout.mileDistance = workout.mileDistance;
                convertedWorkout.milePaceSeconds = workout.milePaceSeconds;
                convertedWorkout.ProgramId = workout.ProgramId;
                convertedWorkout.GoalId = workout.GoalId;
                convertedWorkout.RunType = workout.RunType;

                lastWorkoutCycle.Add(convertedWorkout);
            }

            return lastWorkoutCycle;
        }


        private async Task CleanExerciseWorkoutDatabase()
        {
            Client client = await _repo.Client.GetClientAsync(GetUserId());

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

        public ClientWorkout InstantiateClientWorkout(SingleWorkoutVM workoutVM)
        {
            ClientWorkout workout = new ClientWorkout();
            workout.ClientId = workoutVM.Client.ClientId;
            if (workoutVM.FitnessParameters.cardioComponent != null)
            {
                workout.mileDistance = workoutVM.FitnessParameters.cardioComponent.distanceMiles;
                workout.milePaceSeconds = (int)(workoutVM.FitnessParameters.cardioComponent.milePace * 60);
            }
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

        private List<Exercise> CleanseExerciseDescriptions(List<Exercise> exercises)
        {
            foreach (Exercise exercise in exercises)
            {
                exercise.description = SharedUtility.RemoveTags(exercise.description);
            }

            return exercises;
        }

        //---------------------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------------------


        public async Task<ActionResult> Equipment()
        {
            var client = await _repo.Client.GetClientAsync(GetUserId());
            var clientEquipment = await _repo.ClientEquipment.GetClientEquipmentAsync(client.ClientId);
            var equipment = await _repo.Equipment.GetAllEquipmentAsync();

            ClientEquipmentVM clientEquipmentVM = GetClientEquipmentViewModel(client, clientEquipment, equipment);

            return View(clientEquipmentVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Equipment(ClientEquipmentVM clientEquipmentVM)
        {
            var client = await _repo.Client.GetClientAsync(GetUserId());
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

        public async Task<bool> RecommendAgainstHighIntensity()
        {
            var client = await _repo.Client.GetClientAsync(GetUserId());

            List<ClientEquipment> equipment = await _repo.ClientEquipment.GetClientEquipmentAsync(client.ClientId);
            int[] highIntensityCompatibleEquipment = new int[]{ 1, 2, 3, 8, 9 };
          
            foreach(ClientEquipment piece in equipment)
            {
                if (highIntensityCompatibleEquipment.Contains(piece.EquipmentId))
                {
                    return false;
                }
            }

            return true;
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
                return RedirectToAction("Index", new { accountNewlyCreated = true });
            }
            catch
            {
                return View("Create");
            }
        }

        // GET: Client/Edit/5
        public async Task<ActionResult> Edit()
        {
            Client client = await _repo.Client.GetClientAsync(GetUserId());
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
               
                List<ClientWorkout> clientWorkouts = await _repo.ClientWorkout.GetAllWorkoutsByProgramAsync(clientProgram.ProgramId);
                foreach (ClientWorkout workout in clientWorkouts)
                {
                    List<ClientExercise> clientExercises = await _repo.ClientExercise.GetClientExerciseByWorkoutAsync(workout.Id); 
                    foreach (ClientExercise exercise in clientExercises)
                    {
                        _repo.ClientExercise.DeleteClientExercise(exercise);
                    }
                    _repo.ClientWorkout.DeleteClientWorkout(workout);
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
