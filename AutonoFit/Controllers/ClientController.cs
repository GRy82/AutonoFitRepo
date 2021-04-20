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
        private LiftPrescription liftPrescript;
        private CardioPrescription cardioPrescript;
        private ProgramModule programModule;
        public ClientController(IRepositoryWrapper repo, ExerciseLibraryService exerciseLibraryService)
        {
            _repo = repo;
            _exerciseLibraryService = exerciseLibraryService;
            programModule = new ProgramModule(_repo);
            liftPrescript = new LiftPrescription(_repo, exerciseLibraryService);
            cardioPrescript = new CardioPrescription(_repo, exerciseLibraryService);
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
            List<Exercise> exercises = await liftPrescript.GatherExercises(workoutVM);
            LiftingComponent liftingComponent = new LiftingComponent(SharedUtility.SetTrainingStimuli(workoutVM.GoalIds));
            liftingComponent.SetLiftParameters();
            CardioComponent cardioComponent = SharedUtility.CheckCardio(workoutVM.GoalIds) ? new CardioComponent(workoutVM) : null;
            cardioComponent.SetCardioParameters();
            workoutVM.Minutes = cardioComponent != null ? (workoutVM.Minutes / 2) : workoutVM.Minutes; //if cardio is involved, cut minutes in half to have half the time for cardio.
            workoutVM.LiftingComponent = liftingComponent;
            workoutVM.CardioComponent = cardioComponent;
            int numberOfExercises = SharedUtility.GetExerciseQty(liftingComponent, workoutVM.Minutes); 
            List<Exercise> randomlyChosenExercises = SharedUtility.RandomizeExercises(exercises, numberOfExercises);
            ClientWorkout workout = InstantiateClientWorkout(workoutVM); //Create new workout to contain exercises and other stored data.
            workoutVM.Workout = workout; //assign all ClientExercises the workout Id
            //randomlyChosenExercises = CleanseExerciseDescriptions(randomlyChosenExercises);
            workoutVM.Exercises = randomlyChosenExercises;  //Place exercises in ViewModel

            return View("DisplaySingleWorkout", workoutVM);
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

        public async Task<ActionResult> GenerateProgramWorkout(int programId)
        {
            ClientProgram currentProgram = await _repo.ClientProgram.GetClientProgramAsync(programId);
            int todaysGoalNumber = await liftPrescript.GetTodaysGoal(currentProgram);
            var recentWorkouts = await GatherRecentWorkouts(currentProgram);
            CardioComponent cardioComponent = null;

            bool todayIsCardioGoal = (todaysGoalNumber == 4 || todaysGoalNumber == 5);
            List<ClientWorkout> pastSameGoalWorkouts = FilterWorkoutsByGoal(recentWorkouts, todaysGoalNumber);
            if (todayIsCardioGoal) 
                cardioComponent = await cardioPrescript.GetTodaysCardio(pastSameGoalWorkouts, currentProgram);

            bool supplementalLiftNeeded = cardioComponent != null && (cardioComponent is EasyRun ||
                                                                        cardioComponent is SixLift);
            int liftWorkoutInMinutes = currentProgram.MinutesPerSession;//default value
            var lastWorkoutBodyParts = recentWorkouts?.ElementAt(0).BodyParts;
            string upperOrLowerBody = SetBodyParts(lastWorkoutBodyParts, todayIsCardioGoal, supplementalLiftNeeded, cardioComponent.runType);

            Client client = await _repo.Client.GetClientAsync(GetUserId());
            LiftingComponent liftingComponent = null;
            ClientWorkout clientWorkout = InstantiateClientWorkout(cardioComponent, client, upperOrLowerBody, currentProgram, todaysGoalNumber);
            _repo.ClientWorkout.CreateClientWorkout(clientWorkout);
            await _repo.SaveAsync();

            if (!todayIsCardioGoal || supplementalLiftNeeded)
            {
                var equipment = await _repo.ClientEquipment.GetClientEquipmentAsync(client.ClientId);
                liftingComponent = await liftPrescript.GenerateLiftingComponent(upperOrLowerBody, todaysGoalNumber, clientWorkout,
                                                                     currentProgram, liftWorkoutInMinutes, equipment);
            }

            if (liftingComponent != null)
                CreateExercisesInDatabase(liftingComponent, clientWorkout);

            await _repo.SaveAsync();
            ProgramWorkoutVM programWorkoutVM = InstantiateProgramWorkoutVM(cardioComponent, liftingComponent, clientWorkout);

            return View("DisplayProgramWorkout", programWorkoutVM);
        }
        
        private string SetBodyParts(string lastWorkoutBodyParts, bool todayIsCardioGoal, bool supplementalLiftNeeded, string runType)
        {
            string upperOrLowerBody = "Upper Body";
            if (!todayIsCardioGoal)//if true, determine body parts, then generate lifting component.
                upperOrLowerBody = liftPrescript.GetBodyParts(lastWorkoutBodyParts);//******* CHeck here
            
            if (supplementalLiftNeeded) //if supplemental lift. Easy run accompanied by full body lift. 6-Lift accompanied by upper body.  
                if (runType == "Easy")
                    upperOrLowerBody = "Both";//Must divide liftWorkoutInMinutes by 2 at some point in this case.
                
            return upperOrLowerBody;
        }

        private ClientWorkout InstantiateClientWorkout(CardioComponent cardioComponent, Client client, string bodyParts, ClientProgram currentProgram, int todaysGoalNumber)
        {
            double milePace = 0, distanceMiles = 0;
            string runType = "";

            if(cardioComponent != null)
            {
                milePace = cardioComponent.milePace;
                distanceMiles = cardioComponent.distanceMiles;
                runType = cardioComponent.runType;
            }

            return new ClientWorkout()
            {
                ClientId = client.ClientId,
                ProgramId = currentProgram.ProgramId,
                BodyParts = bodyParts,
                GoalId = todaysGoalNumber,
                RunType = runType,
                milePaceSeconds = Convert.ToInt32(milePace * 60),
                mileDistance = distanceMiles,
                DatePerformed = DateTime.Now
            };
        }

        private void CreateExercisesInDatabase(LiftingComponent liftingComponent, ClientWorkout clientWorkout)
        {
            foreach (var exercise in liftingComponent.exercises)
                _repo.Exercise.CreateExercise(exercise);
        }

        private ProgramWorkoutVM InstantiateProgramWorkoutVM(CardioComponent cardioComponent, LiftingComponent liftingComponent, 
                                                                                                    ClientWorkout clientWorkout)
        {
            return new ProgramWorkoutVM()
            {
                LiftingComponent = liftingComponent ?? null,
                Exercises = liftingComponent != null ? liftingComponent.exercises : null,
                CardioComponent = cardioComponent ?? null,
                ClientWorkout = clientWorkout
            };
        }

        public async Task<ActionResult> CompleteProgramWorkout(ProgramWorkoutVM programWorkoutVM)
        {
            ClientWorkout clientWorkout = await _repo.ClientWorkout.GetClientWorkoutAsync(programWorkoutVM.ClientWorkout.Id);
            clientWorkout.CardioRPE = programWorkoutVM.CardioRPE;
            clientWorkout.Completed = true;
            _repo.ClientWorkout.EditClientWorkout(clientWorkout);
            var exercises = await _repo.Exercise.GetExerciseByWorkoutAsync(programWorkoutVM.ClientWorkout.Id);
            for (int i = 0; i < exercises.Count; i++)
            {
                exercises[i].RPE = programWorkoutVM.RPEs[i];
                _repo.Exercise.EditExercise(exercises[i]);
            }
            
            await _repo.SaveAsync();

            return RedirectToAction("ProgramsList");
        }

        //-------------------------------------------------------------------------------------------------------
        //-----------------------------------Helper Methods----------------------------------------------------

        //Make sure IOrderedEnumerable can be converted to List<ClientWorkout> implicitly.
        public async Task<IOrderedEnumerable<ClientWorkout>> GatherRecentWorkouts(ClientProgram currentProgram)
        {
            List<ClientWorkout> recentWorkouts = await _repo.ClientWorkout.GetAllWorkoutsByProgramAsync(currentProgram.ProgramId);

            if (recentWorkouts.Count == 0)
                return null;

            var workouts = from s in recentWorkouts
                           orderby s.Id descending
                           select s;

            return workouts;
        }

        public List<ClientWorkout> FilterWorkoutsByGoal(IOrderedEnumerable<ClientWorkout> recentWorkouts, int todaysGoalNumber)
        {
            List<ClientWorkout> sameGoalWorkouts = new List<ClientWorkout>();
            if (recentWorkouts == null) return sameGoalWorkouts;

            foreach (var workout in recentWorkouts)
                if (workout.GoalId == todaysGoalNumber)
                    sameGoalWorkouts.Add(workout);

            return sameGoalWorkouts;
        }


        public ClientWorkout InstantiateClientWorkout(SingleWorkoutVM workoutVM)
        {
            ClientWorkout workout = new ClientWorkout();
            workout.ClientId = workoutVM.Client.ClientId;
            if (workoutVM.CardioComponent != null)
            {
                workout.mileDistance = workoutVM.CardioComponent.distanceMiles;
                workout.milePaceSeconds = (int)(workoutVM.CardioComponent.milePace * 60);
            }
            workout.DatePerformed = DateTime.Now;
            return workout;
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
                _repo.ClientEquipment.DeleteClientEquipment(possession);
            
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
                    List<Exercise> clientExercises = await _repo.Exercise.GetExerciseByWorkoutAsync(workout.Id); 
                    foreach (Exercise exercise in clientExercises)
                    {
                        _repo.Exercise.DeleteExercise(exercise);
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
