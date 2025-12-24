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

        public async Task<User?> AuthenticateAsync(string loginId, string password)
        {
            // Try both email and mobile number for login
            return await _userRepository.GetByEmailOrMobileAndPasswordAsync(loginId, password);
        }

        public async Task SeedDefaultUsersAsync()
        {
            if (!await _userRepository.HasUsersAsync())
            {
                await _userRepository.AddAsync(new User 
                { 
                    Username = "admin",
                    FullName = "Administrator",
                    Email = "admin@aerodrox.com",
                    MobileNumber = "1234567890",
                    Password = "admin123",
                    Role = "Admin" 
                });
                await _userRepository.SaveChangesAsync();
            }
        }

        public async Task<bool> CreateUserAsync(string username, string fullName, string email, string mobileNumber, string password, string role)
        {
            // Check if username is unique
            var existingUserByUsername = await _userRepository.GetByUsernameAsync(username);
            if (existingUserByUsername != null)
            {
                return false;
            }

            // Check if email is unique
            var existingUserByEmail = await _userRepository.GetByEmailAsync(email);
            if (existingUserByEmail != null)
            {
                return false;
            }

            // Check if mobile number is unique
            var existingUserByMobile = await _userRepository.GetByMobileNumberAsync(mobileNumber);
            if (existingUserByMobile != null)
            {
                return false;
            }

            var newUser = new User 
            {
                Username = username,
                FullName = fullName,
                Email = email,
                MobileNumber = mobileNumber,
                Password = password,
                Role = role,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(newUser);
            await _userRepository.SaveChangesAsync();
            return true;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllUsersAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _userRepository.GetUserByIdAsync(id);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return false;
            }

            await _userRepository.DeleteUserAsync(user);
            await _userRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateUserAsync(User user, string newUsername, string newFullName, string newEmail, string newMobileNumber, string newPassword, string newRole)
        {
            // Check username uniqueness if changed
            if (user.Username != newUsername)
            {
                var existingUser = await _userRepository.GetByUsernameAsync(newUsername);
                if (existingUser != null && existingUser.Id != user.Id)
                {
                    return false;
                }
                user.Username = newUsername;
            }

            // Check email uniqueness if changed
            if (user.Email != newEmail)
            {
                var existingUser = await _userRepository.GetByEmailAsync(newEmail);
                if (existingUser != null && existingUser.Id != user.Id)
                {
                    return false;
                }
                user.Email = newEmail;
            }

            // Check mobile number uniqueness if changed
            if (user.MobileNumber != newMobileNumber)
            {
                var existingUser = await _userRepository.GetByMobileNumberAsync(newMobileNumber);
                if (existingUser != null && existingUser.Id != user.Id)
                {
                    return false;
                }
                user.MobileNumber = newMobileNumber;
            }

            user.FullName = newFullName;

            if (!string.IsNullOrEmpty(newPassword))
            {
                user.Password = newPassword;
            }

            user.Role = newRole;

            await _userRepository.UpdateUserAsync(user);
            return true;
        }

        public async Task<User?> GetUserByEmailOrMobileAsync(string emailOrMobile)
        {
            return await _userRepository.GetByEmailOrMobileAsync(emailOrMobile);
        }

        // Fixed method - now uses the repository method correctly
        public async Task<bool> UpdateProfileAsync(int userId, string fullName, string email, string mobileNumber, 
                                                   string? address, string? city, string? state, string? pinCode, 
                                                   DateTime? dateOfBirth, string? gender, string? profilePicture)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            // Check email uniqueness if changed
            if (user.Email != email)
            {
                var existingUser = await _userRepository.GetByEmailAsync(email);
                if (existingUser != null && existingUser.Id != user.Id)
                {
                    return false;
                }
                user.Email = email;
            }

            // Check mobile number uniqueness if changed
            if (user.MobileNumber != mobileNumber)
            {
                var existingUser = await _userRepository.GetByMobileNumberAsync(mobileNumber);
                if (existingUser != null && existingUser.Id != user.Id)
                {
                    return false;
                }
                user.MobileNumber = mobileNumber;
            }

            // Update profile fields
            user.FullName = fullName;
            user.Address = address;
            user.City = city;
            user.State = state;
            user.PinCode = pinCode;
            user.DateOfBirth = dateOfBirth;
            user.Gender = gender;
            
            if (!string.IsNullOrEmpty(profilePicture))
            {
                user.ProfilePicture = profilePicture;
            }

            await _userRepository.UpdateProfileAsync(user);
            return true;
        }

        // New method for updating password only
        public async Task<bool> UpdatePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null || user.Password != currentPassword)
            {
                return false;
            }

            user.Password = newPassword;
            await _userRepository.UpdateUserAsync(user);
            return true;
        }
    }
}