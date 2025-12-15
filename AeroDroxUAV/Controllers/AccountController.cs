using AeroDroxUAV.Models;
using AeroDroxUAV.Services; 
using Microsoft.AspNetCore.Mvc;
using System.Linq; 

public class AccountController : Controller
{
    private readonly IUserService _userService; 

    public AccountController(IUserService userService) 
    {
        _userService = userService;
    }

    private void SetNoCacheHeaders()
    {
        // Fix: Use indexer property to set/replace headers, resolving ASP0019 warning.
        Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
        Response.Headers["Pragma"] = "no-cache";
        Response.Headers["Expires"] = "0";
    }

    [HttpGet]
    public IActionResult Login()
    {
        SetNoCacheHeaders();
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("Username")))
        {
            return RedirectToAction("Index", "Drone");
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string Username, string Password)
    {
        SetNoCacheHeaders();
        var user = await _userService.AuthenticateAsync(Username, Password);

        if(user != null)
        {
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);
            return RedirectToAction("Index", "Drone");
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
    
    // ===================================
    // REGISTER ACTIONS
    // ===================================
    
    [HttpGet]
    public IActionResult Register()
    {
        SetNoCacheHeaders();
        
        // Pass a flag to the view to conditionally show role selection options
        ViewBag.IsAdminLoggedIn = HttpContext.Session.GetString("Role") == "Admin";
        
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(string Username, string Password, string Role)
    {
        SetNoCacheHeaders();
        
        string finalRole = "User";

        // Security Logic: Only allow creating an Admin if the current session belongs to an Admin
        if (Role == "Admin")
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                ViewBag.Error = "You do not have permission to create an Admin account.";
                ViewBag.IsAdminLoggedIn = false;
                return View("Register");
            }
            finalRole = "Admin";
        }

        var success = await _userService.CreateUserAsync(Username, Password, finalRole);

        if (success)
        {
            if (finalRole == "Admin")
            {
                 // Admin created by another admin: Stay logged in and redirect
                 // We redirect to the User Management page now instead of Index
                return RedirectToAction("UserManagement"); 
            }
            // Standard User created: Redirect to Login page
            return RedirectToAction("Login");
        }
        else
        {
            ViewBag.Error = "Username already exists. Please choose a different name.";
            ViewBag.IsAdminLoggedIn = HttpContext.Session.GetString("Role") == "Admin";
            return View("Register");
        }
    }
    
    // ===================================
    // NEW: USER MANAGEMENT ACTIONS (ADMIN ONLY)
    // ===================================
    
    [HttpGet]
    public async Task<IActionResult> UserManagement(string filter = "All")
    {
        SetNoCacheHeaders();

        // Check if the user is an Admin
        if (HttpContext.Session.GetString("Role") != "Admin")
        {
            // Not an admin, redirect them away
            return RedirectToAction("Index", "Home"); 
        }

        var allUsers = await _userService.GetAllUsersAsync();
        IEnumerable<User> usersToDisplay = allUsers;
        
        if (filter.Equals("Admins", StringComparison.OrdinalIgnoreCase))
        {
            usersToDisplay = allUsers.Where(u => u.Role == "Admin");
        }
        else if (filter.Equals("Users", StringComparison.OrdinalIgnoreCase))
        {
             usersToDisplay = allUsers.Where(u => u.Role == "User");
        }
        // else: filter is "All", usersToDisplay is allUsers

        ViewBag.CurrentFilter = filter;
        ViewBag.AdminCount = allUsers.Count(u => u.Role == "Admin");
        ViewBag.UserCount = allUsers.Count(u => u.Role == "User");
        ViewBag.TotalCount = allUsers.Count;

        return View(usersToDisplay.ToList());
    }

    // NEW: Implementation for Delete
    [HttpPost]
    public async Task<IActionResult> DeleteUser(int id)
    {
        SetNoCacheHeaders();
        // Security check: Only Admins can delete
        if (HttpContext.Session.GetString("Role") != "Admin")
        {
            return RedirectToAction("Index", "Home"); 
        }

        var userToDelete = await _userService.GetUserByIdAsync(id);
        
        if (userToDelete != null)
        {
            // Prevent Admin from deleting themselves
            if (userToDelete.Username == HttpContext.Session.GetString("Username"))
            {
                TempData["Error"] = "You cannot delete your own admin account.";
            }
            else
            {
                var success = await _userService.DeleteUserAsync(id);
                if (!success)
                {
                    TempData["Error"] = "Failed to delete user.";
                }
                else
                {
                    TempData["Success"] = $"User '{userToDelete.Username}' deleted successfully.";
                }
            }
        }
        else
        {
            TempData["Error"] = "User not found.";
        }

        // Redirect back to the user list
        return RedirectToAction("UserManagement");
    }

    // NEW: Implementation for Edit (GET)
    [HttpGet]
    public async Task<IActionResult> EditUser(int id)
    {
        SetNoCacheHeaders();
        // Security check: Only Admins can edit
        if (HttpContext.Session.GetString("Role") != "Admin")
        {
            return RedirectToAction("Index", "Home"); 
        }

        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            TempData["Error"] = "User not found.";
            return RedirectToAction("UserManagement");
        }
        
        // Return the User model to a view named EditUser.cshtml (which is not provided)
        return View(user); 
    }
    
    // NEW: Implementation for Edit (POST)
    [HttpPost]
    public async Task<IActionResult> EditUser(int Id, string Username, string Password, string Role)
    {
        SetNoCacheHeaders();
        // Security check: Only Admins can edit
        if (HttpContext.Session.GetString("Role") != "Admin")
        {
            return RedirectToAction("Index", "Home"); 
        }

        var userToUpdate = await _userService.GetUserByIdAsync(Id);
        if (userToUpdate == null)
        {
            TempData["Error"] = "User not found for update.";
            return RedirectToAction("UserManagement");
        }

        // Prevent Admin from demoting or changing their own account role/username
        if (userToUpdate.Username == HttpContext.Session.GetString("Username"))
        {
            if (userToUpdate.Role != Role || userToUpdate.Username != Username)
            {
                ViewBag.Error = "You cannot change your own role or username from the edit page.";
                return View(userToUpdate);
            }
        }

        var success = await _userService.UpdateUserAsync(userToUpdate, Username, Password, Role);

        if (success)
        {
            TempData["Success"] = $"User '{Username}' updated successfully.";
            // If the currently logged-in user updated their own password, clear session and re-login
            if (userToUpdate.Username == HttpContext.Session.GetString("Username") && !string.IsNullOrEmpty(Password))
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login");
            }
        }
        else
        {
            ViewBag.Error = "Failed to update user. The new username might already exist.";
            return View(userToUpdate); // Return to the edit view with error
        }

        return RedirectToAction("UserManagement");
    }
}