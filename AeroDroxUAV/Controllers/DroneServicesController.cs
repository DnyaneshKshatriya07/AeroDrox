using AeroDroxUAV.Models;
using AeroDroxUAV.Services; 
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace AeroDroxUAV.Controllers
{
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class DroneServicesController : Controller
    {
        private readonly IDroneServicesService _droneServicesService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DroneServicesController(IDroneServicesService droneServicesService, IWebHostEnvironment webHostEnvironment) 
        {
            _droneServicesService = droneServicesService;
            _webHostEnvironment = webHostEnvironment;
        }

        // Security Helpers
        private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";
        private bool IsLoggedIn() => !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));

        // View all drone services for everyone
        public async Task<IActionResult> Index()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var droneServicesList = await _droneServicesService.GetActiveDroneServicesAsync(); 
            ViewBag.Role = HttpContext.Session.GetString("Role");
            return View(droneServicesList); 
        }

        // Action to view details of a single drone service
        public async Task<IActionResult> Details(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var droneService = await _droneServicesService.GetDroneServicesByIdAsync(id);

            if (droneService == null) return NotFound();

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
        public async Task<IActionResult> Create(DroneServices droneServices, IFormFile imageFile)
        {
            if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();

            if(ModelState.IsValid)
            {
                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    droneServices.ImageUrl = await UploadImageAsync(imageFile);
                }

                await _droneServicesService.CreateDroneServicesAsync(droneServices);
                return RedirectToAction("Index");
            }
            return View(droneServices); 
        }

        public async Task<IActionResult> Edit(int id)
        {
            if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();

            var droneServices = await _droneServicesService.GetDroneServicesByIdAsync(id);
            if(droneServices == null) return NotFound();
            return View(droneServices);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(DroneServices droneServices, IFormFile imageFile)
        {
            if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();

            if(ModelState.IsValid)
            {
                // Handle image upload if new image is provided
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(droneServices.ImageUrl))
                    {
                        DeleteImage(droneServices.ImageUrl);
                    }
                    droneServices.ImageUrl = await UploadImageAsync(imageFile);
                }

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

            var droneService = await _droneServicesService.GetDroneServicesByIdAsync(id);
            if (droneService != null && !string.IsNullOrEmpty(droneService.ImageUrl))
            {
                DeleteImage(droneService.ImageUrl);
            }

            await _droneServicesService.DeleteDroneServicesAsync(id);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();

            await _droneServicesService.ToggleServiceStatusAsync(id);
            return RedirectToAction("Index");
        }

        // Private helper methods for image handling
        private async Task<string> UploadImageAsync(IFormFile imageFile)
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "services");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return $"/images/services/{uniqueFileName}";
        }

        private void DeleteImage(string imageUrl)
        {
            if (!string.IsNullOrEmpty(imageUrl))
            {
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, imageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }
        }
    }
}