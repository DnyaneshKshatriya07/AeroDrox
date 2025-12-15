namespace AeroDroxUAV.Services
{
    using AeroDroxUAV.Models;
    using AeroDroxUAV.Repositories;
    
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            return await _userRepository.GetByUsernameAndPasswordAsync(username, password);
        }

        public async Task SeedDefaultUsersAsync()
        {
            if (!await _userRepository.HasUsersAsync())
            {
                await _userRepository.AddAsync(new User { Username = "admin", Password = "admin123", Role = "Admin" });
                await _userRepository.AddAsync(new User { Username = "user", Password = "user123", Role = "User" });
                await _userRepository.SaveChangesAsync();
            }
        }
        
        public async Task<bool> CreateUserAsync(string username, string password, string role)
        {
            // Check if the username is unique
            var existingUser = await _userRepository.GetByUsernameAsync(username); 
            if (existingUser != null)
            {
                return false; // User already exists
            }
            
            var newUser = new User 
            {
                Username = username,
                Password = password,
                Role = role
            };

            await _userRepository.AddAsync(newUser);
            await _userRepository.SaveChangesAsync();
            return true;
        }

        // NEW: Implementation for getting all users
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllUsersAsync();
        }
        
        // NEW: Implementation for getting a user by ID
        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _userRepository.GetUserByIdAsync(id);
        }
        
        // NEW: Implementation for deleting a user
        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return false; // User not found
            }

            await _userRepository.DeleteUserAsync(user);
            await _userRepository.SaveChangesAsync();
            return true;
        }

        // NEW: Implementation for updating a user
        public async Task<bool> UpdateUserAsync(User user, string newUsername, string newPassword, string newRole)
        {
            // 1. Handle Username update (check for uniqueness if it changed)
            if (user.Username != newUsername)
            {
                var existingUser = await _userRepository.GetByUsernameAsync(newUsername);
                if (existingUser != null)
                {
                    return false; // New username is already taken
                }
                user.Username = newUsername;
            }

            // 2. Handle Password update (only if a new password is provided)
            if (!string.IsNullOrEmpty(newPassword))
            {
                user.Password = newPassword;
            }

            // 3. Handle Role update
            user.Role = newRole;

            // 4. Update in repository and save changes
            await _userRepository.UpdateUserAsync(user);
            await _userRepository.SaveChangesAsync();
            return true;
        }
    }
}