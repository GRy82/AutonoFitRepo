﻿using AutonoFit.Classes;
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
        private SingleModule singleModule;
        public ClientController(IRepositoryWrapper repo, ExerciseLibraryService exerciseLibraryService)
        {
            _repo = repo;
            programModule = new ProgramModule(_repo);
            _exerciseLibraryService = exerciseLibraryService;
            singleModule = new SingleModule(_repo, exerciseLibraryService);
        }

        // GET: Client
        public async Task<ActionResult> Index(bool accountNewlyCreated = false)
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = await _repo.Client.GetClientAsync(userId);
          
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
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            workoutVM.Client = await _repo.Client.GetClientAsync(userId);
            workoutVM.Equipment = await _repo.ClientEquipment.GetClientEquipmentAsync(workoutVM.Client.ClientId);
            List<Result> exerciseResults = await GatherExercises(workoutVM);
            FitnessDictionary fitnessMetrics = singleModule.CalculateSetsRepsRest(workoutVM.GoalIds, workoutVM.Minutes, workoutVM.MileMinutes, workoutVM.MileSeconds); //Calculate sets/reps, rest time to exercises.
            workoutVM.Minutes = fitnessMetrics.cardio == true ? (workoutVM.Minutes / 2) : workoutVM.Minutes; //if cardio is involved, cut minutes in half to have half the time for cardio.
            int numberOfExercises = SharedUtility.DetermineVolume(fitnessMetrics, workoutVM.Minutes); //Decide number of exercises based on time constraints 
            List<Result> randomlyChosenExercises = SharedUtility.RandomizeExercises(exerciseResults, numberOfExercises); //Randomly select N number of exercises from total collection thus far. 
            List<ClientExercise> workoutExercises = SharedUtility.CopyAsClientExercises(randomlyChosenExercises, workoutVM, fitnessMetrics);  //Convert ExerciseLibrary objects to ClientExercises
            workoutVM.fitnessDictionary = fitnessMetrics.cardio == true ? SharedUtility.ConvertFitnessDictCardioValues(fitnessMetrics) : fitnessMetrics;
            ClientWorkout workout = InstantiateClientWorkout(workoutVM); //Create new workout to contain exercises and other stored data.
            workoutVM.Workout = workout; //assign all ClientExercises the workout Id
            randomlyChosenExercises = CleanExerciseDescriptions(randomlyChosenExercises);
            workoutVM.Exercises = randomlyChosenExercises;  //Place exercises in ViewModel

            return View("DisplaySingleWorkout", workoutVM);
        }

        private async Task<List<Result>> GatherExercises(SingleWorkoutVM workoutVM)
        {
            List<Result> exerciseResults = await singleModule.FindExercisesByCategory(workoutVM, new List<Result> { }); //Get exercises by category and repackage into Result reference type.
            exerciseResults = await singleModule.FindExercisesByMuscles(workoutVM, exerciseResults); //Get exercises by muslces and repackage into Result reference type.
            exerciseResults = SharedUtility.RemoveRepeats(exerciseResults); //Get rid of repeats
            return exerciseResults;
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
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = await _repo.Client.GetClientAsync(userId);

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
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            Client client = await _repo.Client.GetClientAsync(userId);
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

        private async Task<List<Result>> GetExercisesByEquipment(Client client)
        {
            List<ClientEquipment> equipment = await _repo.ClientEquipment.GetClientEquipmentAsync(client.ClientId);
            string url = SharedUtility.BuildEquipmentUrlString(equipment);
            ExerciseLibrary exerciseLibrary = await _exerciseLibraryService.GetExercises(url);
            return SharedUtility.RepackageResults(new List<Result>(), exerciseLibrary);
        }

        public async Task<ActionResult> GenerateProgramWorkout(int programId)
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            Client client = await _repo.Client.GetClientAsync(userId);
            List<Result> resultsLibrary = await GetExercisesByEquipment(client);

            List<Result> todaysExercises = new List<Result> { };
            ClientProgram currentProgram = await _repo.ClientProgram.GetClientProgramAsync(programId);
            List<FitnessDictionary> fitnessMetrics = new List<FitnessDictionary> { };
            List<ClientWorkout> recentWorkoutCycle = await GatherWorkoutCycle(currentProgram);
            List<int> goalIds = new List<int> { currentProgram.GoalOneId, Convert.ToInt32(currentProgram.GoalTwoId) };
            List<ClientExercise> clientExercises = new List<ClientExercise> { };
            int todaysGoalNumber = programModule.GetTodaysGoal(recentWorkoutCycle, goalIds, currentProgram.GoalCount);
            string bodyParts = null;
            
            if (todaysGoalNumber == 4 || todaysGoalNumber == 5) {
                fitnessMetrics.Add(await programModule.GetTodaysCardio(new FitnessDictionary(), recentWorkoutCycle, todaysGoalNumber, currentProgram));
            }
            if(todaysGoalNumber != 4 && todaysGoalNumber != 5 || (fitnessMetrics.Count != 0 && (fitnessMetrics[0].runType == "Easy" || fitnessMetrics[0].runType == "6-Lift")))
            {
                int liftLengthMinutes = 0;
                int totalExerciseTime = 0;
                if(todaysGoalNumber != 4 && todaysGoalNumber != 5)// if it is a purely lifitng workout
                {
                    bodyParts = programModule.GetBodyParts(recentWorkoutCycle, todaysGoalNumber, currentProgram.GoalCount);
                    liftLengthMinutes = currentProgram.MinutesPerSession;
                }
                else//if supplemental cardio is needed or a lift for someone who does too much cardio("6 Lift")
                {
                    bodyParts = fitnessMetrics[0].runType == "Easy" ? "Both" : "Upper Body";
                    liftLengthMinutes = fitnessMetrics[0].runType == "Easy" ? Convert.ToInt32(fitnessMetrics[0].runDuration) : currentProgram.MinutesPerSession;
                }
                while(totalExerciseTime < liftLengthMinutes)
                {
                    Result exercise = SharedUtility.SelectExercise(bodyParts, resultsLibrary, todaysExercises);
                    exercise.description = SharedUtility.RemoveTags(exercise.description);
                    todaysExercises.Add(exercise);
                    FitnessDictionary tempFitDict = new FitnessDictionary();
                    tempFitDict = await programModule.GenerateLift(currentProgram, recentWorkoutCycle, tempFitDict, todaysGoalNumber, exercise.id);
                    fitnessMetrics.Add(tempFitDict);
                    ClientExercise clienteExercise = SharedUtility.CopyAsClientExercises(exercise, client.ClientId, tempFitDict);
                    clientExercises.Add(clienteExercise);
                    totalExerciseTime += (int)(SharedUtility.GetSingleExerciseTime(tempFitDict) / 60);
                } 
            }

            ClientWorkout clientWorkout = InstantiateClientWorkout(fitnessMetrics, client, bodyParts, currentProgram, todaysGoalNumber);
            _repo.ClientWorkout.CreateClientWorkout(clientWorkout);
            AddClientExercises(clientExercises, currentProgram, clientWorkout);
            await _repo.SaveAsync();

            ProgramWorkoutVM programWorkoutVM = InstantiateProgramWorkoutVM(clientExercises, todaysExercises, fitnessMetrics, clientWorkout);

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

        private ClientWorkout InstantiateClientWorkout(List<FitnessDictionary> fitnessMetrics, Client client, string bodyParts, ClientProgram currentProgram, int todaysGoalNumber)
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

        private ProgramWorkoutVM InstantiateProgramWorkoutVM(List<ClientExercise> clientExercises, List<Result> todaysExercises, List<FitnessDictionary> fitnessMetrics, ClientWorkout clientWorkout)
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

        public ClientWorkout InstantiateClientWorkout(SingleWorkoutVM workoutVM)
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

        public async Task<bool> RecommendAgainstHighIntensity()
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = await _repo.Client.GetClientAsync(userId);

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
