using AeroDroxUAV.Models;
using AeroDroxUAV.Services; 
using Microsoft.AspNetCore.Mvc;

namespace AeroDroxUAV.Controllers
{
    // A better approach is to use the [Authorize(Roles = "Admin")] attribute
    // on the controller and specific methods, but using session check for this 
    // structure.
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    // NOTE: Add [Route] or [Controller] attribute if using API endpoints
    public class AccessoriesController : Controller
    {
        private readonly IAccessoriesService _accessoriesService; 

        public AccessoriesController(IAccessoriesService accessoriesService) 
        {
            _accessoriesService = accessoriesService;
        }

        // Security Helpers - Ensure you have 'using Microsoft.AspNetCore.Http;' if needed elsewhere
        private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";
        private bool IsLoggedIn() => !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));


        // View all accessories
        public async Task<IActionResult> Index()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var accessoriess = await _accessoriesService.GetAllAccessoriesAsync();
            ViewBag.Role = HttpContext.Session.GetString("Role");
            return View(accessoriess);
        }

        // ========== ADMIN CRUD ONLY ==========

        public IActionResult Create()
        {
            if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Accessories accessories)
        {
            if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();

            if(ModelState.IsValid)
            {
                await _accessoriesService.CreateAccessoriesAsync(accessories);
                return RedirectToAction("Index");
            }
            return View(accessories);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();

            var accessories = await _accessoriesService.GetAccessoriesByIdAsync(id);
            if(accessories == null) return NotFound();
            return View(accessories);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Accessories accessories) // Corrected parameter name
        {
            if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();

            if(ModelState.IsValid)
            {
                await _accessoriesService.UpdateAccessoriesAsync(accessories); // Corrected variable
                return RedirectToAction("Index");
            }
            return View(accessories); // Corrected variable
        }

        public async Task<IActionResult> Delete(int id)
        {
            if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();

            var accessories = await _accessoriesService.GetAccessoriesByIdAsync(id);
            if(accessories == null) return NotFound();
            return View(accessories);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();

            await _accessoriesService.DeleteAccessoriesAsync(id);
            
            return RedirectToAction("Index");
        }
    }
}