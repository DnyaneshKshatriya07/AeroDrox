namespace AeroDroxUAV.Repositories
{
    using AeroDroxUAV.Data;
    using AeroDroxUAV.Models;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;

    public class DroneRepository : IDroneRepository
    {
        private readonly AppDbContext _context;

        public DroneRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Drone>> GetAllAsync()
        {
            return await _context.Drones.ToListAsync();
        }

        public async Task<Drone?> GetByIdAsync(int id)
        {
            return await _context.Drones.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task AddAsync(Drone drone)
        {
            await _context.Drones.AddAsync(drone);
        }

        public void Update(Drone drone)
        {
            _context.Drones.Update(drone);
        }

        public void Delete(Drone drone)
        {
            _context.Drones.Remove(drone);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        // Additional helper methods if needed
        public async Task<IEnumerable<Drone>> GetByCategoryAsync(string category)
        {
            return await _context.Drones
                .Where(d => d.Category == category)
                .ToListAsync();
        }

        public async Task<IEnumerable<Drone>> GetFeaturedAsync()
        {
            return await _context.Drones
                .Where(d => d.IsFeatured)
                .ToListAsync();
        }

        // NEW METHOD: Get drones for homepage
        public async Task<IEnumerable<Drone>> GetHomepageDronesAsync()
        {
            return await _context.Drones
                .Where(d => d.ShowOnHomepage)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }
    }
}