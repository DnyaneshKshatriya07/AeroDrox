using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AeroDroxUAV.Models;
using AeroDroxUAV.Services; 
using Microsoft.AspNetCore.Http;

namespace AeroDroxUAV.Controllers;

[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IDroneServicesService _droneServicesService;
    private readonly IDroneService _droneService;
    private readonly IAccessoriesService _accessoriesService;

    public HomeController(ILogger<HomeController> logger, 
                         IDroneServicesService droneServicesService,
                         IDroneService droneService,
                         IAccessoriesService accessoriesService)
    {
        _logger = logger;
        _droneServicesService = droneServicesService;
        _droneService = droneService;
        _accessoriesService = accessoriesService;
    }

    private bool IsLoggedIn() => !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "AeroDroxUAV - Precision Aerial Solutions";
        
        // Initialize ViewBag collections to empty lists to avoid null references
        ViewBag.FeaturedDrones = new List<Drone>();
        ViewBag.NewDrones = new List<Drone>();
        ViewBag.FeaturedAccessories = new List<Accessories>();
        ViewBag.NewAccessories = new List<Accessories>();
        ViewBag.FeaturedServices = new List<DroneServices>();
        ViewBag.TotalDrones = 0;
        ViewBag.TotalAccessories = 0;
        
        try
        {
            // Get featured drones (customized drones)
            var allDrones = await _droneService.GetAllDronesAsync();
            if (allDrones != null)
            {
                ViewBag.FeaturedDrones = allDrones.Where(d => d.IsFeatured).Take(6).ToList();
                
                // UPDATED: Get New Arrivals - only show drones with ShowOnHomepage = true
                ViewBag.NewDrones = allDrones
                    .Where(d => d.ShowOnHomepage) // Only drones marked for homepage
                    .OrderByDescending(d => d.CreatedAt)
                    .Take(8)
                    .ToList();
                    
                ViewBag.TotalDrones = allDrones.Count();
            }
            
            // Get accessories
            var allAccessories = await _accessoriesService.GetAllAccessoriesAsync();
            if (allAccessories != null)
            {
                ViewBag.FeaturedAccessories = allAccessories.Take(8).ToList();
                ViewBag.NewAccessories = allAccessories.OrderByDescending(a => a.CreatedAt).Take(8).ToList();
                ViewBag.TotalAccessories = allAccessories.Count();
            }
            
            // Get drone services for single-card carousel
            var featuredServices = await _droneServicesService.GetAllDroneServicesAsync();
            if (featuredServices != null)
            {
                // Take enough services for carousel
                ViewBag.FeaturedServices = featuredServices.Take(10).ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading data for home page");
        }
        
        ViewBag.IsLoggedIn = IsLoggedIn();
        ViewBag.Role = HttpContext.Session.GetString("Role") ?? "";
        
        return View(); 
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}