using AeroDroxUAV.Models;
using AeroDroxUAV.Services; 
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using System.Text.RegularExpressions;

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

    [HttpGet]
    public IActionResult BuyProduct(int productId, string productType = "drone")
    {
        HttpContext.Session.SetString("BuyProductId", productId.ToString());
        HttpContext.Session.SetString("BuyProductType", productType);
        
        TempData["Message"] = "Please login to complete your purchase.";
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult BookService(int serviceId)
    {
        HttpContext.Session.SetString("BookServiceId", serviceId.ToString());
        
        TempData["Message"] = "Please login to book this service.";
        return RedirectToAction("Login");
    }

    [HttpPost]
    public async Task<IActionResult> Login(string LoginId, string Password)
    {
        SetNoCacheHeaders();
        
        // Server-side validation
        var errors = new List<string>();
        
        if (string.IsNullOrEmpty(LoginId))
        {
            errors.Add("Email is required.");
        }
        else if (!IsValidEmail(LoginId))
        {
            errors.Add("Please enter a valid email address.");
        }
        
        if (string.IsNullOrEmpty(Password))
        {
            errors.Add("Password is required.");
        }
        
        if (errors.Any())
        {
            ViewBag.Error = string.Join(" ", errors);
            return View();
        }

        var user = await _userService.AuthenticateAsync(LoginId, Password);

        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _userService.UpdateUserAsync(user, user.Username, user.FullName, user.Email, user.MobileNumber, "", user.Role);
            
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("FullName", user.FullName);
            HttpContext.Session.SetString("Email", user.Email);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetInt32("UserId", user.Id); // This stores as integer, not string
            
            var buyProductId = HttpContext.Session.GetString("BuyProductId");
            var buyProductType = HttpContext.Session.GetString("BuyProductType");
            var bookServiceId = HttpContext.Session.GetString("BookServiceId");
            
            HttpContext.Session.Remove("BuyProductId");
            HttpContext.Session.Remove("BuyProductType");
            HttpContext.Session.Remove("BookServiceId");
            
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
            ViewBag.Error = "Invalid email or password";
            return View();
        }
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult Register()
    {
        SetNoCacheHeaders();
        
        ViewBag.IsAdminLoggedIn = HttpContext.Session.GetString("Role") == "Admin";
        
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(string Username, string FullName, string Email, string MobileNumber, string Password, string ConfirmPassword, string Role)
    {
        SetNoCacheHeaders();
        
        // Comprehensive validation
        var errors = new List<string>();
        
        // Validate Full Name
        if (string.IsNullOrEmpty(FullName))
        {
            errors.Add("Full name is required.");
        }
        else if (!Regex.IsMatch(FullName, @"^[a-zA-Z\s]+$"))
        {
            errors.Add("Full name can only contain letters and spaces.");
        }
        else if (FullName.Length > 40)
        {
            errors.Add("Full name cannot exceed 40 characters.");
        }
        
        // Validate Username
        if (string.IsNullOrEmpty(Username))
        {
            errors.Add("Username is required.");
        }
        else if (!Regex.IsMatch(Username, @"^[a-zA-Z0-9]+$"))
        {
            errors.Add("Username can only contain letters and numbers (no symbols).");
        }
        else if (Username.Length > 15)
        {
            errors.Add("Username cannot exceed 15 characters.");
        }
        else
        {
            // Check duplicate username
            var existingUser = await _userService.GetUserByEmailOrMobileAsync(Username);
            if (existingUser != null)
            {
                errors.Add("Username already exists. Please choose a different username.");
            }
        }
        
        // Validate Email
        if (string.IsNullOrEmpty(Email))
        {
            errors.Add("Email is required.");
        }
        else if (!IsValidEmail(Email))
        {
            errors.Add("Please enter a valid email address.");
        }
        else
        {
            // Check duplicate email
            var existingEmail = await _userService.GetUserByEmailOrMobileAsync(Email);
            if (existingEmail != null)
            {
                errors.Add("Email already registered. Please use a different email or login.");
            }
        }
        
        // Validate Mobile Number
        if (string.IsNullOrEmpty(MobileNumber))
        {
            errors.Add("Mobile number is required.");
        }
        else if (!Regex.IsMatch(MobileNumber, @"^[6-9][0-9]{9}$"))
        {
            errors.Add("Please enter a valid 10-digit mobile number starting with 6-9.");
        }
        else
        {
            // Check duplicate mobile number
            var existingMobile = await _userService.GetUserByEmailOrMobileAsync(MobileNumber);
            if (existingMobile != null)
            {
                errors.Add("Mobile number already registered. Please use a different number.");
            }
        }
        
        // Validate Password
        if (string.IsNullOrEmpty(Password))
        {
            errors.Add("Password is required.");
        }
        else if (!IsValidPassword(Password))
        {
            errors.Add("Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one number, and one special character.");
        }
        
        // Validate Confirm Password
        if (string.IsNullOrEmpty(ConfirmPassword))
        {
            errors.Add("Please confirm your password.");
        }
        else if (Password != ConfirmPassword)
        {
            errors.Add("Passwords do not match.");
        }
        
        if (errors.Any())
        {
            ViewBag.Error = string.Join(" ", errors);
            ViewBag.IsAdminLoggedIn = HttpContext.Session.GetString("Role") == "Admin";
            return View("Register");
        }

        string finalRole = "User";

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
                return RedirectToAction("UserManagement");
            }
            TempData["Success"] = "Registration successful! Please login with your email.";
            return RedirectToAction("Login");
        }
        else
        {
            ViewBag.Error = "Registration failed. Please try again.";
            ViewBag.IsAdminLoggedIn = HttpContext.Session.GetString("Role") == "Admin";
            return View("Register");
        }
    }

    [HttpGet]
    public async Task<IActionResult> UserManagement(string filter = "All")
    {
        SetNoCacheHeaders();

        if (HttpContext.Session.GetString("Role") != "Admin")
        {
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
        if (HttpContext.Session.GetString("Role") != "Admin")
        {
            return RedirectToAction("Index", "Home");
        }

        var userToDelete = await _userService.GetUserByIdAsync(id);

        if (userToDelete != null)
        {
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

        return RedirectToAction("UserManagement");
    }

    [HttpGet]
    public async Task<IActionResult> EditUser(int id)
    {
        SetNoCacheHeaders();
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

        var errors = new List<string>();

        if (string.IsNullOrEmpty(FullName))
        {
            errors.Add("Full name is required.");
        }
        else if (!Regex.IsMatch(FullName, @"^[a-zA-Z\s]+$"))
        {
            errors.Add("Full name can only contain letters and spaces.");
        }
        else if (FullName.Length > 40)
        {
            errors.Add("Full name cannot exceed 40 characters.");
        }

        if (string.IsNullOrEmpty(Email))
        {
            errors.Add("Email is required.");
        }
        else if (!IsValidEmail(Email))
        {
            errors.Add("Please enter a valid email address.");
        }

        if (string.IsNullOrEmpty(MobileNumber))
        {
            errors.Add("Mobile number is required.");
        }
        else if (!Regex.IsMatch(MobileNumber, @"^[6-9][0-9]{9}$"))
        {
            errors.Add("Please enter a valid 10-digit mobile number starting with 6-9.");
        }

        if (errors.Any())
        {
            TempData["Error"] = string.Join(" ", errors);
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
            null
        );

        if (success)
        {
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

        var errors = new List<string>();

        if (string.IsNullOrEmpty(CurrentPassword))
        {
            errors.Add("Current password is required.");
        }

        if (string.IsNullOrEmpty(NewPassword))
        {
            errors.Add("New password is required.");
        }
        else if (!IsValidPassword(NewPassword))
        {
            errors.Add("New password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one number, and one special character.");
        }

        if (string.IsNullOrEmpty(ConfirmPassword))
        {
            errors.Add("Please confirm your new password.");
        }
        else if (NewPassword != ConfirmPassword)
        {
            errors.Add("New passwords do not match.");
        }

        if (errors.Any())
        {
            TempData["PasswordError"] = string.Join(" ", errors);
            return RedirectToAction("MyProfile");
        }

        var success = await _userService.UpdatePasswordAsync(userId.Value, CurrentPassword, NewPassword);
        
        if (success)
        {
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

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var fileExtension = Path.GetExtension(profilePicture.FileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(fileExtension))
        {
            TempData["Error"] = "Only JPG, JPEG, PNG, and GIF files are allowed.";
            return RedirectToAction("MyProfile");
        }

        if (profilePicture.Length > 5 * 1024 * 1024)
        {
            TempData["Error"] = "Profile picture must be less than 5MB.";
            return RedirectToAction("MyProfile");
        }

        try
        {
            var fileName = $"profile_{userId.Value}_{Guid.NewGuid()}{fileExtension}";
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
            
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await profilePicture.CopyToAsync(stream);
            }

            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user != null)
            {
                if (!string.IsNullOrEmpty(user.ProfilePicture))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePicture.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

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

    private bool IsValidPassword(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 8)
            return false;
            
        bool hasUpper = password.Any(char.IsUpper);
        bool hasLower = password.Any(char.IsLower);
        bool hasDigit = password.Any(char.IsDigit);
        bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));
        
        return hasUpper && hasLower && hasDigit && hasSpecial;
    }

    [HttpGet]
    public IActionResult TermsAndConditions()
    {
        return View();
    }
}