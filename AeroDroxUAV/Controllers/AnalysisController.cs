using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AeroDroxUAV.Models;

namespace AeroDroxUAV.Controllers;

[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class AnalysisController : Controller
{
    private readonly ILogger<AnalysisController> _logger;

    public AnalysisController(ILogger<AnalysisController> logger)
    {
        _logger = logger;
    }

    private bool IsLoggedIn() => !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));

    public IActionResult Index()
    {
        // Analytics dashboard should still require login
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        
        // And should be admin-only
        if (HttpContext.Session.GetString("Role") != "Admin")
            return RedirectToAction("Index", "Home");
            
        return View(); 
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}