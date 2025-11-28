using AeroDroxUAV.Data;
using AeroDroxUAV.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AeroDroxUAV.Controllers
{
    // 1. Mark as API Controller: Enables API-specific conventions (like automatic 400 response)
    [Route("api/[controller]")]
    [ApiController] 
    public class DronesApiController : ControllerBase 
    {
        private readonly AppDbContext _context;

        public DronesApiController(AppDbContext context)
        {
            _context = context;
        }

        // Helper to check if a user is logged in via Session (for simplicity)
        private bool IsLoggedIn() => !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));
        private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";

        // GET: api/DronesApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Drone>>> GetDrones()
        {
            if (!IsLoggedIn())
            {
                return Unauthorized(); // Returns 401
            }
            return await _context.Drones.ToListAsync(); // Returns 200 OK with JSON list
        }

        // GET: api/DronesApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Drone>> GetDrone(int id)
        {
            if (!IsLoggedIn())
            {
                return Unauthorized();
            }

            var drone = await _context.Drones.FindAsync(id);

            if (drone == null)
            {
                return NotFound(); // Returns 404
            }

            return drone; // Returns 200 OK with single JSON object
        }

        // POST: api/DronesApi
        [HttpPost]
        public async Task<ActionResult<Drone>> PostDrone(Drone drone)
        {
            if (!IsLoggedIn() || !IsAdmin())
            {
                return Forbid(); // Returns 403 (Logged in but wrong role) or 401
            }

            _context.Drones.Add(drone);
            await _context.SaveChangesAsync();

            // Returns 201 CreatedAtAction with the created resource and its URL
            return CreatedAtAction(nameof(GetDrone), new { id = drone.Id }, drone);
        }

        // PUT: api/DronesApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDrone(int id, Drone drone)
        {
            if (id != drone.Id)
            {
                return BadRequest(); // Returns 400
            }

            if (!IsLoggedIn() || !IsAdmin())
            {
                return Forbid();
            }

            _context.Entry(drone).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Drones.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent(); // Returns 204 success, no content
        }

        // DELETE: api/DronesApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDrone(int id)
        {
            if (!IsLoggedIn() || !IsAdmin())
            {
                return Forbid();
            }

            var drone = await _context.Drones.FindAsync(id);
            if (drone == null)
            {
                return NotFound();
            }

            _context.Drones.Remove(drone);
            await _context.SaveChangesAsync();

            return NoContent(); // Returns 204 success
        }
    }
}