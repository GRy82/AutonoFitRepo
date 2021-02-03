using AutonoFit.Contracts;
using AutonoFit.Models;
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
        public ClientController(IRepositoryWrapper repo)
        {
            _repo = repo;
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

        public async Task<ActionResult> SingleWorkoutSetup(bool errorMessage = false)
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
        public async Task<ActionResult> GenerateSingleWorkout(SingleWorkoutVM singleWorkoutVM)
        {
            if(singleWorkoutVM.GoalIds[0] == 0 && singleWorkoutVM.GoalIds[1] == 0)//Client selected no goals.
            {
                return RedirectToAction("SingleWorkoutSetup", new RouteValueDictionary( new { controller = "Client", 
                    action = "SingleWorkoutSetup", errorMessage = true }));
            }
            return RedirectToAction("Index");
        }


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
