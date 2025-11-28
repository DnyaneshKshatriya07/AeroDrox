using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AeroDroxUAV.Models;

namespace AeroDroxUAV.Controllers;

// Fix: Corrected 'ResponseCacheCacheLocation' to 'ResponseCacheLocation'
// Apply anti-caching to Home pages as well
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    private bool IsLoggedIn() => !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));

    public IActionResult Index()
    {
        // If not logged in, redirect to login
        if (!IsLoggedIn())
        {
            return RedirectToAction("Login", "Account");
        }
        
        // If logged in, redirect them to the main application page
        return RedirectToAction("Index", "Drone");
    }

    public IActionResult Privacy()
    {
        // This page remains accessible to all, though often it's protected if tied to user data.
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}