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
        
        // Get featured drones
        var allDrones = await _droneService.GetAllDronesAsync();
        var featuredDrones = allDrones.Where(d => d.IsFeatured).Take(6).ToList();
        var newDrones = allDrones.OrderByDescending(d => d.CreatedAt).Take(6).ToList();
        
        // Get accessories
        var allAccessories = await _accessoriesService.GetAllAccessoriesAsync();
        var featuredAccessories = allAccessories.Take(6).ToList();
        var newAccessories = allAccessories.OrderByDescending(a => a.CreatedAt).Take(6).ToList();
        
        ViewBag.FeaturedDrones = featuredDrones;
        ViewBag.NewDrones = newDrones;
        ViewBag.FeaturedAccessories = featuredAccessories;
        ViewBag.NewAccessories = newAccessories;
        ViewBag.TotalDrones = allDrones.Count();
        ViewBag.TotalAccessories = allAccessories.Count();
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
