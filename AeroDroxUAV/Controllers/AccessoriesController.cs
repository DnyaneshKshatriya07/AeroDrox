using AeroDroxUAV.Models;
using AeroDroxUAV.Services; 
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace AeroDroxUAV.Controllers
{
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class AccessoriesController : Controller
    {
        private readonly IAccessoriesService _accessoriesService; 
        private readonly IWebHostEnvironment _environment;

        public AccessoriesController(IAccessoriesService accessoriesService, IWebHostEnvironment environment) 
        {
            _accessoriesService = accessoriesService;
            _environment = environment;
        }

        // Security Helpers
        private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";
        private bool IsLoggedIn() => !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));

        // View all accessories
        public async Task<IActionResult> Index()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var accessories = await _accessoriesService.GetAllAccessoriesAsync();
            ViewBag.Role = HttpContext.Session.GetString("Role");
            return View(accessories);
        }

        // Accessory Details
        public async Task<IActionResult> Details(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var accessory = await _accessoriesService.GetAccessoriesByIdAsync(id);
            if (accessory == null) return NotFound();
            return View(accessory);
        }

        // ========== ADMIN CRUD ONLY ==========

        public IActionResult Create()
        {
            if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Accessories accessory)
        {
            if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();

            if(ModelState.IsValid)
            {
                // Handle image upload
                if (accessory.ImageFile != null && accessory.ImageFile.Length > 0)
                {
                    // Ensure uploads directory exists
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "accessories");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Generate unique filename
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(accessory.ImageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save the file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await accessory.ImageFile.CopyToAsync(fileStream);
                    }

                    // Set the ImageUrl - use forward slash for web URLs
                    accessory.ImageUrl = $"/uploads/accessories/{uniqueFileName}";
                }
                else
                {
                    // Set default image if no file uploaded
                    accessory.ImageUrl = "/images/default-accessory.jpg";
                }

                accessory.CreatedAt = DateTime.Now;
                await _accessoriesService.CreateAccessoriesAsync(accessory);
                return RedirectToAction("Index");
            }
            
            // If we got this far, something failed, redisplay form
            return View(accessory);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();

            var accessory = await _accessoriesService.GetAccessoriesByIdAsync(id);
            if(accessory == null) return NotFound();
            return View(accessory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Accessories accessory)
        {
            if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();

            if(ModelState.IsValid)
            {
                var existingAccessory = await _accessoriesService.GetAccessoriesByIdAsync(accessory.Id);
                if (existingAccessory == null) return NotFound();

                // Handle image upload
                if (accessory.ImageFile != null && accessory.ImageFile.Length > 0)
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(existingAccessory.ImageUrl) && 
                        existingAccessory.ImageUrl != "/images/default-accessory.jpg")
                    {
                        var oldImagePath = Path.Combine(_environment.WebRootPath, existingAccessory.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Save new image
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "accessories");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(accessory.ImageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await accessory.ImageFile.CopyToAsync(fileStream);
                    }

                    accessory.ImageUrl = $"/uploads/accessories/{uniqueFileName}";
                }
                else
                {
                    // Keep existing image
                    accessory.ImageUrl = existingAccessory.ImageUrl;
                }

                await _accessoriesService.UpdateAccessoriesAsync(accessory);
                return RedirectToAction("Index");
            }
            return View(accessory);
        }

        public async Task<IActionResult> Delete(int id)
        {
            if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();

            var accessory = await _accessoriesService.GetAccessoriesByIdAsync(id);
            if(accessory == null) return NotFound();
            return View(accessory);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();

            var accessory = await _accessoriesService.GetAccessoriesByIdAsync(id);
            if (accessory != null)
            {
                // Delete image file if exists and not default
                if (!string.IsNullOrEmpty(accessory.ImageUrl) && 
                    accessory.ImageUrl != "/images/default-accessory.jpg")
                {
                    var imagePath = Path.Combine(_environment.WebRootPath, accessory.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }
            }

            await _accessoriesService.DeleteAccessoriesAsync(id);
            return RedirectToAction("Index");
        }
    }
}