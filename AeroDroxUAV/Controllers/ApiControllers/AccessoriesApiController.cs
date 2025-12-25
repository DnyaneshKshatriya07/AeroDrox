// AccessoriesApiController.cs
using AeroDroxUAV.Models;
using AeroDroxUAV.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace AeroDroxUAV.Controllers
{
    [Route("api/[controller]")]
    [ApiController] 
    public class AccessoriesApiController : ControllerBase 
    {
        private readonly IAccessoriesService _accessoriesService; 

        public AccessoriesApiController(IAccessoriesService accessoriesService)
        {
            _accessoriesService = accessoriesService;
        }

        // Helper methods for session/authorization (if uncommented)
        private bool IsLoggedIn() => !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));
        private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";

        // GET: api/AccessoriesApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Accessories>>> GetAccessories()
        {
            // if (!IsLoggedIn()) { return Unauthorized(); } // <-- Temporarily commented
            
            var accessories = await _accessoriesService.GetAllAccessoriesAsync(); 
            return Ok(accessories);
        }

        // GET: api/AccessoriesApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Accessories>> GetAccessory(int id)
        {
            // if (!IsLoggedIn()) { return Unauthorized(); } // <-- Temporarily commented

            var accessory = await _accessoriesService.GetAccessoriesByIdAsync(id);

            if (accessory == null)
            {
                return NotFound();
            }

            return Ok(accessory);
        }

        // POST: api/AccessoriesApi
        [HttpPost]
        public async Task<ActionResult<Accessories>> PostAccessory(Accessories accessories)
        {
            // if (!IsLoggedIn() || !IsAdmin()) { return Forbid(); } // <-- Temporarily commented

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            accessories.CreatedAt = DateTime.Now;
            await _accessoriesService.CreateAccessoriesAsync(accessories);

            return CreatedAtAction(nameof(GetAccessory), new { id = accessories.Id }, accessories);
        }

        // PUT: api/AccessoriesApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAccessory(int id, Accessories accessories)
        {
            if (id != accessories.Id)
            {
                return BadRequest();
            }

            // if (!IsLoggedIn() || !IsAdmin()) { return Forbid(); } // <-- Temporarily commented
            
            var existingAccessory = await _accessoriesService.GetAccessoriesByIdAsync(id);
            if (existingAccessory == null)
            {
                return NotFound();
            }

            try
            {
                await _accessoriesService.UpdateAccessoriesAsync(accessories);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _accessoriesService.GetAccessoriesByIdAsync(id) == null)
                {
                    return NotFound();
                }
                throw; 
            }

            return NoContent();
        }

        // DELETE: api/AccessoriesApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccessory(int id)
        {
            // if (!IsLoggedIn() || !IsAdmin()) { return Forbid(); } // <-- Temporarily commented

            await _accessoriesService.DeleteAccessoriesAsync(id);
            
            return NoContent();
        }
    }
}