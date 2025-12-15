using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AeroDroxUAV.Models;
using AeroDroxUAV.Services; // Add this using statement

namespace AeroDroxUAV.Controllers;

[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IDroneServicesService _droneServicesService; // Injected Service

    // Constructor updated to include IDroneServicesService
    public HomeController(ILogger<HomeController> logger, IDroneServicesService droneServicesService)
    {
        _logger = logger;
        _droneServicesService = droneServicesService;
    }

    private bool IsLoggedIn() => !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));

    public IActionResult Index()
    {
        // If logged in, they can still view the public-facing single-page website.
        // We are removing the redirect to Drone/Index to allow the home page to be accessible via the navbar link.
        ViewData["Title"] = "AeroDroxUAV - Precision Aerial Solutions";
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