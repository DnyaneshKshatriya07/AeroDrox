using AeroDroxUAV.Models;
using AeroDroxUAV.Services; 
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;

public class AccountController : Controller
{
    private readonly IUserService _userService;

    public AccountController(IUserService userService)
    {
        _userService = userService;
    }

    private void SetNoCacheHeaders()
    {
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

    // [HttpPost]
    // public async Task<IActionResult> Login(string LoginId, string Password)
    // {
    //     SetNoCacheHeaders();
    //     var user = await _userService.AuthenticateAsync(LoginId, Password);

    //     if(user != null)
    //     {
    //         // Update last login time
    //         user.LastLoginAt = DateTime.UtcNow;
    //         await _userService.UpdateUserAsync(user, user.Username, user.FullName, user.Email, user.MobileNumber, "", user.Role);
            
    //         HttpContext.Session.SetString("Username", user.Username);
    //         HttpContext.Session.SetString("FullName", user.FullName);
    //         HttpContext.Session.SetString("Email", user.Email);
    //         HttpContext.Session.SetString("Role", user.Role);
    //         HttpContext.Session.SetInt32("UserId", user.Id);
            
    //         return RedirectToAction("Index", "Drone");
    //     }
    //     else
    //     {
    //         ViewBag.Error = "Invalid email/mobile number or password";
    //         return View();
    //     }
    // }

    // ===================================
// BUY AND BOOK ACTIONS
// ===================================

[HttpGet]
public IActionResult BuyProduct(int productId, string productType = "drone")
{
    // Store the intended product to buy in session
    HttpContext.Session.SetString("BuyProductId", productId.ToString());
    HttpContext.Session.SetString("BuyProductType", productType);
    
    // Redirect to login
    TempData["Message"] = "Please login to complete your purchase.";
    return RedirectToAction("Login");
}

[HttpGet]
public IActionResult BookService(int serviceId)
{
    // Store the intended service to book in session
    HttpContext.Session.SetString("BookServiceId", serviceId.ToString());
    
    // Redirect to login
    TempData["Message"] = "Please login to book this service.";
    return RedirectToAction("Login");
}

// Update Login POST action to redirect after login
[HttpPost]
public async Task<IActionResult> Login(string LoginId, string Password)
{
    SetNoCacheHeaders();
    var user = await _userService.AuthenticateAsync(LoginId, Password);

    if(user != null)
    {
        // Update last login time
        user.LastLoginAt = DateTime.UtcNow;
        await _userService.UpdateUserAsync(user, user.Username, user.FullName, user.Email, user.MobileNumber, "", user.Role);
        
        HttpContext.Session.SetString("Username", user.Username);
        HttpContext.Session.SetString("FullName", user.FullName);
        HttpContext.Session.SetString("Email", user.Email);
        HttpContext.Session.SetString("Role", user.Role);
        HttpContext.Session.SetInt32("UserId", user.Id);
        
        // Check if user was trying to buy something before login
        var buyProductId = HttpContext.Session.GetString("BuyProductId");
        var buyProductType = HttpContext.Session.GetString("BuyProductType");
        var bookServiceId = HttpContext.Session.GetString("BookServiceId");
        
        // Clear the session values
        HttpContext.Session.Remove("BuyProductId");
        HttpContext.Session.Remove("BuyProductType");
        HttpContext.Session.Remove("BookServiceId");
        
        // Redirect to appropriate page
        if (!string.IsNullOrEmpty(buyProductId))
        {
            if (buyProductType == "accessory")
                return RedirectToAction("Details", "Accessories", new { id = int.Parse(buyProductId) });
            else
                return RedirectToAction("Details", "Drone", new { id = int.Parse(buyProductId) });
        }
        else if (!string.IsNullOrEmpty(bookServiceId))
        {
            return RedirectToAction("Details", "DroneServices", new { id = int.Parse(bookServiceId) });
        }
        
        return RedirectToAction("Index", "Drone");
    }
    else
    {
        ViewBag.Error = "Invalid email/mobile number or password";
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
    public async Task<IActionResult> Register(string Username, string FullName, string Email, string MobileNumber, string Password, string ConfirmPassword, string Role)
    {
        SetNoCacheHeaders();
        
        // Validate passwords match
        if (Password != ConfirmPassword)
        {
            ViewBag.Error = "Passwords do not match.";
            ViewBag.IsAdminLoggedIn = HttpContext.Session.GetString("Role") == "Admin";
            return View("Register");
        }

        // Validate email format
        if (!IsValidEmail(Email))
        {
            ViewBag.Error = "Please enter a valid email address.";
            ViewBag.IsAdminLoggedIn = HttpContext.Session.GetString("Role") == "Admin";
            return View("Register");
        }

        // Validate mobile number format (basic validation)
        if (!IsValidMobileNumber(MobileNumber))
        {
            ViewBag.Error = "Please enter a valid 10-digit mobile number.";
            ViewBag.IsAdminLoggedIn = HttpContext.Session.GetString("Role") == "Admin";
            return View("Register");
        }

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

        var success = await _userService.CreateUserAsync(Username, FullName, Email, MobileNumber, Password, finalRole);

        if (success)
        {
            if (finalRole == "Admin")
            {
                // Admin created by another admin: Stay logged in and redirect
                return RedirectToAction("UserManagement");
            }
            // Standard User created: Redirect to Login page
            TempData["Success"] = "Registration successful! Please login with your email or mobile number.";
            return RedirectToAction("Login");
        }
        else
        {
            ViewBag.Error = "Username, email, or mobile number already exists. Please choose different credentials.";
            ViewBag.IsAdminLoggedIn = HttpContext.Session.GetString("Role") == "Admin";
            return View("Register");
        }
    }

    // ===================================
    // USER MANAGEMENT ACTIONS (ADMIN ONLY)
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

        ViewBag.CurrentFilter = filter;
        ViewBag.AdminCount = allUsers.Count(u => u.Role == "Admin");
        ViewBag.UserCount = allUsers.Count(u => u.Role == "User");
        ViewBag.TotalCount = allUsers.Count;

        return View(usersToDisplay.ToList());
    }

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

        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> EditUser(int Id, string Username, string FullName, string Email, string MobileNumber, string Password, string Role)
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

        var success = await _userService.UpdateUserAsync(userToUpdate, Username, FullName, Email, MobileNumber, Password, Role);

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
            ViewBag.Error = "Failed to update user. The new username, email, or mobile number might already exist.";
            return View(userToUpdate);
        }

        return RedirectToAction("UserManagement");
    }

    // ===================================
    // MY PROFILE ACTIONS
    // ===================================

    [HttpGet]
    public async Task<IActionResult> MyProfile()
    {
        SetNoCacheHeaders();
        
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("Username")))
        {
            return RedirectToAction("Login");
        }

        var userId = HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue)
        {
            return RedirectToAction("Login");
        }

        var user = await _userService.GetUserByIdAsync(userId.Value);
        if (user == null)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> MyProfile(string FullName, string Email, string MobileNumber, 
                                               string Address, string City, string State, 
                                               string PinCode, DateTime? DateOfBirth, string Gender)
    {
        SetNoCacheHeaders();
        
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("Username")))
        {
            return RedirectToAction("Login");
        }

        var userId = HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue)
        {
            return RedirectToAction("Login");
        }

        // Validate email format
        if (!IsValidEmail(Email))
        {
            TempData["Error"] = "Please enter a valid email address.";
            var user = await _userService.GetUserByIdAsync(userId.Value);
            return View(user);
        }

        // Validate mobile number format
        if (!IsValidMobileNumber(MobileNumber))
        {
            TempData["Error"] = "Please enter a valid 10-digit mobile number.";
            var user = await _userService.GetUserByIdAsync(userId.Value);
            return View(user);
        }

        var success = await _userService.UpdateProfileAsync(
            userId.Value,
            FullName,
            Email,
            MobileNumber,
            Address,
            City,
            State,
            PinCode,
            DateOfBirth,
            Gender,
            null // We'll handle profile picture separately
        );

        if (success)
        {
            // Update session data
            HttpContext.Session.SetString("FullName", FullName);
            HttpContext.Session.SetString("Email", Email);
            
            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("MyProfile");
        }
        else
        {
            TempData["Error"] = "Failed to update profile. The new email or mobile number might already be in use.";
            var user = await _userService.GetUserByIdAsync(userId.Value);
            return View(user);
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdatePassword(string CurrentPassword, string NewPassword, string ConfirmPassword)
    {
        SetNoCacheHeaders();
        
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("Username")))
        {
            return RedirectToAction("Login");
        }

        var userId = HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue)
        {
            return RedirectToAction("Login");
        }

        if (NewPassword != ConfirmPassword)
        {
            TempData["PasswordError"] = "New passwords do not match.";
            return RedirectToAction("MyProfile");
        }

        if (string.IsNullOrEmpty(NewPassword) || NewPassword.Length < 6)
        {
            TempData["PasswordError"] = "New password must be at least 6 characters long.";
            return RedirectToAction("MyProfile");
        }

        var success = await _userService.UpdatePasswordAsync(userId.Value, CurrentPassword, NewPassword);
        
        if (success)
        {
            TempData["Success"] = "Password updated successfully!";
            // Clear session and redirect to login if user changed their own password
            if (HttpContext.Session.GetInt32("UserId") == userId.Value)
            {
                HttpContext.Session.Clear();
                TempData["Success"] = "Password updated successfully! Please login again with your new password.";
                return RedirectToAction("Login");
            }
        }
        else
        {
            TempData["PasswordError"] = "Current password is incorrect.";
        }

        return RedirectToAction("MyProfile");
    }

    [HttpPost]
    public async Task<IActionResult> UploadProfilePicture(IFormFile profilePicture)
    {
        SetNoCacheHeaders();
        
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("Username")))
        {
            return RedirectToAction("Login");
        }

        var userId = HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue)
        {
            return RedirectToAction("Login");
        }

        if (profilePicture == null || profilePicture.Length == 0)
        {
            TempData["Error"] = "Please select a profile picture.";
            return RedirectToAction("MyProfile");
        }

        // Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var fileExtension = Path.GetExtension(profilePicture.FileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(fileExtension))
        {
            TempData["Error"] = "Only JPG, JPEG, PNG, and GIF files are allowed.";
            return RedirectToAction("MyProfile");
        }

        // Validate file size (max 5MB)
        if (profilePicture.Length > 5 * 1024 * 1024)
        {
            TempData["Error"] = "Profile picture must be less than 5MB.";
            return RedirectToAction("MyProfile");
        }

        try
        {
            // Generate unique filename
            var fileName = $"profile_{userId.Value}_{Guid.NewGuid()}{fileExtension}";
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
            
            // Create directory if it doesn't exist
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await profilePicture.CopyToAsync(stream);
            }

            // Get user and update profile picture path
            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user != null)
            {
                // Delete old profile picture if exists
                if (!string.IsNullOrEmpty(user.ProfilePicture))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePicture.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Update profile picture in database
                var profilePicturePath = $"/uploads/profiles/{fileName}";
                var success = await _userService.UpdateProfileAsync(
                    userId.Value,
                    user.FullName,
                    user.Email,
                    user.MobileNumber,
                    user.Address,
                    user.City,
                    user.State,
                    user.PinCode,
                    user.DateOfBirth,
                    user.Gender,
                    profilePicturePath
                );

                if (success)
                {
                    TempData["Success"] = "Profile picture updated successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to update profile picture in database.";
                }
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error uploading profile picture: {ex.Message}";
        }

        return RedirectToAction("MyProfile");
    }

    // Helper methods for validation
    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private bool IsValidMobileNumber(string mobileNumber)
    {
        // Basic validation for 10-digit number
        return !string.IsNullOrEmpty(mobileNumber) && 
               mobileNumber.Length == 10 && 
               mobileNumber.All(char.IsDigit);
    }
}