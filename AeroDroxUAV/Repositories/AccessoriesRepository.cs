namespace AeroDroxUAV.Repositories
{
    using AeroDroxUAV.Data;
    using AeroDroxUAV.Models;
    using Microsoft.EntityFrameworkCore;

    public class AccessoriesRepository : IAccessoriesRepository
    {
        private readonly AppDbContext _context;

        public AccessoriesRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Accessories>> GetAllAsync()
        {
            return await _context.Accessories.ToListAsync();
        }

        public async Task<Accessories?> GetByIdAsync(int id)
        {
            return await _context.Accessories.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Accessories?> GetByIdForOrderAsync(int id)
        {
            // For order creation - no tracking to avoid conflicts
            return await _context.Accessories.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Accessories?> GetByIdForUpdateAsync(int id)
        {
            // For stock updates - with tracking
            return await _context.Accessories.FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task AddAsync(Accessories accessories)
        {
            await _context.Accessories.AddAsync(accessories);
        }

        public void Update(Accessories accessories)
        {
            _context.Accessories.Update(accessories);
        }

        public async Task UpdateAndSaveAsync(Accessories accessories)
        {
            _context.Accessories.Update(accessories);
            await _context.SaveChangesAsync();
        }

        public void Delete(Accessories accessories)
        {
            _context.Accessories.Remove(accessories);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        // NEW METHOD: Get accessories for homepage
        public async Task<IEnumerable<Accessories>> GetHomepageAccessoriesAsync()
        {
            return await _context.Accessories
                .Where(a => a.ShowOnHomepage)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }
    }
}