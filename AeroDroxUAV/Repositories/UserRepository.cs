namespace AeroDroxUAV.Repositories
{
    using AeroDroxUAV.Data;
    using AeroDroxUAV.Models;
    using Microsoft.EntityFrameworkCore;

    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByUsernameAndPasswordAsync(string username, string password)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.Password == password);
        }

        public async Task<User?> GetByEmailOrMobileAndPasswordAsync(string emailOrMobile, string password)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => 
                    (u.Email == emailOrMobile || u.MobileNumber == emailOrMobile) 
                    && u.Password == password);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByMobileNumberAsync(string mobileNumber)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.MobileNumber == mobileNumber);
        }

        public async Task<User?> GetByEmailOrMobileAsync(string emailOrMobile)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == emailOrMobile || u.MobileNumber == emailOrMobile);
        }

        public async Task<bool> HasUsersAsync()
        {
            return await _context.Users.AnyAsync();
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users.OrderByDescending(u => u.CreatedAt).ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task DeleteUserAsync(User user)
        {
            _context.Users.Remove(user);
            // Add await to make this truly async
            await Task.CompletedTask;
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            // Add await to make this truly async
            await Task.CompletedTask;
        }
    }
}