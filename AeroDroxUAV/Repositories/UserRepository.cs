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
            await _context.SaveChangesAsync(); // Add await here
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync(); // Add await here
        }

        // Updated method to properly update profile
        public async Task UpdateProfileAsync(User user)
        {
            var existingUser = await _context.Users.FindAsync(user.Id);
            if (existingUser != null)
            {
                // Update only profile fields
                existingUser.FullName = user.FullName;
                existingUser.Email = user.Email;
                existingUser.MobileNumber = user.MobileNumber;
                existingUser.Address = user.Address;
                existingUser.City = user.City;
                existingUser.State = user.State;
                existingUser.PinCode = user.PinCode;
                existingUser.DateOfBirth = user.DateOfBirth;
                existingUser.Gender = user.Gender;
                existingUser.ProfilePicture = user.ProfilePicture;
                
                _context.Users.Update(existingUser);
                await _context.SaveChangesAsync(); // Add await here
            }
        }
    }
}
