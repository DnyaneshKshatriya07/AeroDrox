using AeroDroxUAV.Data;
using AeroDroxUAV.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AeroDroxUAV.Controllers
{
    // Fix: Corrected 'ResponseCacheCacheLocation' to 'ResponseCacheLocation'
    // Ensure all actions in DroneController are NEVER cached by the browser
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class DroneController : Controller
    {
        private readonly AppDbContext _context;
        public DroneController(AppDbContext context)
        {
            _context = context;
        }

        // Helper to check for Admin role
        private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";
        
        // New Helper to check if a user is logged in
        private bool IsLoggedIn() => !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));

        // View all drones for everyone (now protected)
        public async Task<IActionResult> Index()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var drones = await _context.Drones.ToListAsync();
            ViewBag.Role = HttpContext.Session.GetString("Role");
            return View(drones);
        }

        // ========== ADMIN CRUD ONLY ==========

        public IActionResult Create()
        {
            if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Drone drone)
        {
            if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();

            if(ModelState.IsValid)
            {
                _context.Drones.Add(drone);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(drone);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();

            var drone = await _context.Drones.FindAsync(id);
            if(drone == null) return NotFound();
            return View(drone);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Drone drone)
        {
            if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();

            if(ModelState.IsValid)
            {
                _context.Drones.Update(drone);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(drone);
        }

        public async Task<IActionResult> Delete(int id)
        {
            if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();

            var drone = await _context.Drones.FindAsync(id);
            if(drone == null) return NotFound();
            return View(drone);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if(!IsLoggedIn() || !IsAdmin()) return Unauthorized();

            var drone = await _context.Drones.FindAsync(id);
            if(drone != null)
            {
                _context.Drones.Remove(drone);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}