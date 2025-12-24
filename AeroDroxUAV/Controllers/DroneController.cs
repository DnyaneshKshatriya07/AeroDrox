using AeroDroxUAV.Models;
using AeroDroxUAV.Services; 
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace AeroDroxUAV.Controllers
{
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class DroneController : Controller
{
    private readonly IDroneService _droneService; 
    private readonly IWebHostEnvironment _environment;

    public DroneController(IDroneService droneService, IWebHostEnvironment environment) 
    {
        _droneService = droneService;
        _environment = environment;
    }

    // Security Helpers
    private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";
    private bool IsLoggedIn() => !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));

    // View all drones for everyone - NO LOGIN REQUIRED
    public async Task<IActionResult> Index()
    {
        var drones = await _droneService.GetAllDronesAsync();
        ViewBag.Role = HttpContext.Session.GetString("Role") ?? "";
        ViewBag.IsLoggedIn = IsLoggedIn();
        return View(drones);
    }

    // Drone Details - NO LOGIN REQUIRED
    public async Task<IActionResult> Details(int id)
    {
        var drone = await _droneService.GetDroneByIdAsync(id);
        if (drone == null) return NotFound();
        ViewBag.IsLoggedIn = IsLoggedIn();
        ViewBag.Role = HttpContext.Session.GetString("Role") ?? "";
        return View(drone);
    }

    // ========== ADMIN CRUD ONLY ==========
    // These actions still require login and admin role

    public IActionResult Create()
    {
        if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Drone drone)
    {
        if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();

        if(ModelState.IsValid)
        {
            // Handle image upload
            if (drone.ImageFile != null && drone.ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "drones");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + drone.ImageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await drone.ImageFile.CopyToAsync(fileStream);
                }

                drone.ImageUrl = $"/uploads/drones/{uniqueFileName}";
            }

            drone.CreatedAt = DateTime.Now;
            await _droneService.CreateDroneAsync(drone);
            return RedirectToAction("Index");
        }
        return View(drone);
    }

    public async Task<IActionResult> Edit(int id)
    {
        if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();

        var drone = await _droneService.GetDroneByIdAsync(id);
        if(drone == null) return NotFound();
        return View(drone);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Drone drone)
    {
        if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();

        if(ModelState.IsValid)
        {
            var existingDrone = await _droneService.GetDroneByIdAsync(drone.Id);
            if (existingDrone == null) return NotFound();

            // Handle image upload
            if (drone.ImageFile != null && drone.ImageFile.Length > 0)
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(existingDrone.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_environment.WebRootPath, existingDrone.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "drones");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + drone.ImageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await drone.ImageFile.CopyToAsync(fileStream);
                }

                drone.ImageUrl = $"/uploads/drones/{uniqueFileName}";
            }
            else
            {
                // Keep existing image
                drone.ImageUrl = existingDrone.ImageUrl;
            }

            await _droneService.UpdateDroneAsync(drone);
            return RedirectToAction("Index");
        }
        return View(drone);
    }

    public async Task<IActionResult> Delete(int id)
    {
        if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();

        var drone = await _droneService.GetDroneByIdAsync(id);
        if(drone == null) return NotFound();
        return View(drone);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();

        var drone = await _droneService.GetDroneByIdAsync(id);
        if (drone != null)
        {
            // Delete image file if exists
            if (!string.IsNullOrEmpty(drone.ImageUrl))
            {
                var imagePath = Path.Combine(_environment.WebRootPath, drone.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                    System.IO.File.Delete(imagePath);
            }
        }

        await _droneService.DeleteDroneAsync(id);
        return RedirectToAction("Index");
    }
}
}