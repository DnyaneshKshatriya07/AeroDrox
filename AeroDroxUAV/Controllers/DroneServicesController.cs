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

    public DroneServicesController(IDroneServicesService droneServicesService) 
    {
        _droneServicesService = droneServicesService;
    }

    // Security Helpers
    private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";
    private bool IsLoggedIn() => !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));

    // View all services - NO LOGIN REQUIRED
    public async Task<IActionResult> Index()
    {
        var droneServicesList = await _droneServicesService.GetAllDroneServicesAsync(); 
        ViewBag.Role = HttpContext.Session.GetString("Role") ?? "";
        ViewBag.IsLoggedIn = IsLoggedIn();
        return View(droneServicesList); 
    }

    // View details of a single drone service - NO LOGIN REQUIRED
    public async Task<IActionResult> Details(int id)
    {
        var droneService = await _droneServicesService.GetDroneServicesByIdAsync(id);

        if (droneService == null) return NotFound();

        ViewBag.Role = HttpContext.Session.GetString("Role") ?? "";
        ViewBag.IsLoggedIn = IsLoggedIn();
        
        return View(droneService);
    }

    // ========== ADMIN CRUD ONLY ==========
    // These actions still require login and admin role

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