// DroneServicesApiController.cs
using AeroDroxUAV.Models;
using AeroDroxUAV.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace AeroDroxUAV.Controllers
{
    [Route("api/[controller]")]
    [ApiController] 
    public class DroneServicesApiController : ControllerBase 
    {
        private readonly IDroneServicesService _droneServicesService; 

        public DroneServicesApiController(IDroneServicesService droneServicesService)
        {
            _droneServicesService = droneServicesService;
        }

        private bool IsLoggedIn() => !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));
        private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";

        // GET: api/DroneServicesApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DroneServices>>> GetDroneServices()
        {
            // if (!IsLoggedIn()) { return Unauthorized(); }
            
            var droneServicesList = await _droneServicesService.GetAllDroneServicesAsync(); 
            return Ok(droneServicesList);
        }

        // GET: api/DroneServicesApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DroneServices>> GetDroneService(int id)
        {
            // if (!IsLoggedIn()) { return Unauthorized(); }

            var droneService = await _droneServicesService.GetDroneServicesByIdAsync(id);

            if (droneService == null)
            {
                return NotFound();
            }

            return Ok(droneService);
        }

        // POST: api/DroneServicesApi
        [HttpPost]
        public async Task<ActionResult<DroneServices>> PostDroneService(DroneServices droneService)
        {
            // if (!IsLoggedIn() || !IsAdmin()) { return Forbid(); }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            await _droneServicesService.CreateDroneServicesAsync(droneService);

            return CreatedAtAction(nameof(GetDroneService), new { id = droneService.Id }, droneService);
        }

        // PUT: api/DroneServicesApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDroneService(int id, DroneServices droneService)
        {
            if (id != droneService.Id)
            {
                return BadRequest();
            }

            // if (!IsLoggedIn() || !IsAdmin()) { return Forbid(); }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var existingDroneService = await _droneServicesService.GetDroneServicesByIdAsync(id);
            if (existingDroneService == null)
            {
                return NotFound();
            }

            try
            {
                await _droneServicesService.UpdateDroneServicesAsync(droneService);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _droneServicesService.GetDroneServicesByIdAsync(id) == null)
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/DroneServicesApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDroneService(int id)
        {
            // if (!IsLoggedIn() || !IsAdmin()) { return Forbid(); }

            await _droneServicesService.DeleteDroneServicesAsync(id);
            
            return NoContent();
        }
    }
}