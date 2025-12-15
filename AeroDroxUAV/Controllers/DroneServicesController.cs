// using AeroDroxUAV.Models;
// using AeroDroxUAV.Services; 
// using Microsoft.AspNetCore.Mvc;

// namespace AeroDroxUAV.Controllers
// {
//     [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
//     public class DroneServicesController : Controller
//     {
//         private readonly IDroneServicesService _droneServicesService; 

//         public DroneServicesController(IDroneServicesService droneServicesService) 
//         {
//             _droneServicesService = droneServicesService;
//         }

//         // Security Helpers
//         private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";
//         private bool IsLoggedIn() => !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));


//         // View all drones for everyone
//         public async Task<IActionResult> Index()
//         {
//             if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

//             var droneServicesList = await _droneServicesService.GetAllDroneServicesAsync(); 
//             ViewBag.Role = HttpContext.Session.GetString("Role");
//             return View(droneServicesList); 
//         }

//         // ========== ADMIN CRUD ONLY ==========

//         public IActionResult Create()
//         {
//             if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();
//             return View();
//         }

//         [HttpPost]
//         public async Task<IActionResult> Create(DroneServices droneServices)
//         {
//             if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();

//             if(ModelState.IsValid)
//             {
//                 await _droneServicesService.CreateDroneServicesAsync(droneServices);
//                 return RedirectToAction("Index");
//             }
//             return View(droneServices); 
//         }

//         public async Task<IActionResult> Edit(int id)
//         {
//             if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();

//             // This is OK because we just pass the object to the view, not immediately update it.
//             var droneServices = await _droneServicesService.GetDroneServicesByIdAsync(id);
//             if(droneServices == null) return NotFound();
//             return View(droneServices);
//         }

//         [HttpPost]
//         public async Task<IActionResult> Edit(DroneServices droneServices)
//         {
//             if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();

//             if(ModelState.IsValid)
//             {
//                 await _droneServicesService.UpdateDroneServicesAsync(droneServices);
//                 return RedirectToAction("Index");
//             }
//             return View(droneServices);
//         }

//         public async Task<IActionResult> Delete(int id)
//         {
//             if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();

//             var droneServices = await _droneServicesService.GetDroneServicesByIdAsync(id);
//             if(droneServices == null) return NotFound();
//             return View(droneServices);
//         }

//         [HttpPost, ActionName("Delete")]
//         public async Task<IActionResult> DeleteConfirmed(int id)
//         {
//             if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();

//             await _droneServicesService.DeleteDroneServicesAsync(id);
            
//             return RedirectToAction("Index");
//         }
//     }
// }

using AeroDroxUAV.Models;
using AeroDroxUAV.Services; 
using Microsoft.AspNetCore.Mvc;

namespace AeroDroxUAV.Controllers
{
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class DroneServicesController : Controller
    {
        private readonly IDroneServicesService _droneServicesService; 

        public DroneServicesController(IDroneServicesService droneServicesService) 
        {
            _droneServicesService = droneServicesService;
        }

        // Security Helpers
        private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";
        private bool IsLoggedIn() => !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));


        // View all drones for everyone
        public async Task<IActionResult> Index()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var droneServicesList = await _droneServicesService.GetAllDroneServicesAsync(); 
            ViewBag.Role = HttpContext.Session.GetString("Role");
            return View(droneServicesList); 
        }

        // ADDED: Action to view details of a single drone service
        public async Task<IActionResult> Details(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var droneService = await _droneServicesService.GetDroneServicesByIdAsync(id);

            if (droneService == null) return NotFound();

            // Optionally, set ViewBag.Role if needed in the Details view
            ViewBag.Role = HttpContext.Session.GetString("Role");
            
            return View(droneService);
        }


        // ========== ADMIN CRUD ONLY ==========

        public IActionResult Create()
        {
            if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(DroneServices droneServices)
        {
            if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();

            if(ModelState.IsValid)
            {
                await _droneServicesService.CreateDroneServicesAsync(droneServices);
                return RedirectToAction("Index");
            }
            return View(droneServices); 
        }

        public async Task<IActionResult> Edit(int id)
        {
            if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();

            // This is OK because we just pass the object to the view, not immediately update it.
            var droneServices = await _droneServicesService.GetDroneServicesByIdAsync(id);
            if(droneServices == null) return NotFound();
            return View(droneServices);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(DroneServices droneServices)
        {
            if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();

            if(ModelState.IsValid)
            {
                await _droneServicesService.UpdateDroneServicesAsync(droneServices);
                return RedirectToAction("Index");
            }
            return View(droneServices);
        }

        public async Task<IActionResult> Delete(int id)
        {
            if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();

            var droneServices = await _droneServicesService.GetDroneServicesByIdAsync(id);
            if(droneServices == null) return NotFound();
            return View(droneServices);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();

            await _droneServicesService.DeleteDroneServicesAsync(id);
            
            return RedirectToAction("Index");
        }
    }
}