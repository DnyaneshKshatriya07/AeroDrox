using AeroDroxUAV.Data;
using AeroDroxUAV.Models;
using Microsoft.AspNetCore.Mvc;

public class AccountController : Controller
{
    private readonly AppDbContext _db;

    public AccountController(AppDbContext db)
    {
        _db = db;
    }

    // Helper method to apply anti-caching headers
    private void SetNoCacheHeaders()
    {
        Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
        Response.Headers.Add("Pragma", "no-cache");
        Response.Headers.Add("Expires", "0");
    }

    [HttpGet]
    public IActionResult Login()
    {
        // Prevent caching of the Login page itself
        SetNoCacheHeaders();

        // Redirect if already logged in (This is the primary way to prevent access)
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("Username")))
        {
            return RedirectToAction("Index", "Drone");
        }
        
        return View();
    }

    [HttpPost]
    public IActionResult Login(string Username, string Password)
    {
        // Prevent caching of the Login page itself
        SetNoCacheHeaders();

        // Check if already logged in
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("Username")))
        {
            return RedirectToAction("Index", "Drone");
        }

        var user = _db.Users.FirstOrDefault(u => u.Username == Username && u.Password == Password);

        if(user != null)
        {
            // Save session
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);

            return RedirectToAction("Index", "Drone"); // Redirect to Drone list
        }
        else
        {
            ViewBag.Error = "Invalid username or password";
            return View();
        }
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}