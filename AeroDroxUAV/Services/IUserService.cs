namespace AeroDroxUAV.Services
{
    using AeroDroxUAV.Models;

    public interface IUserService
    {
        Task<User?> AuthenticateAsync(string loginId, string password);
        Task SeedDefaultUsersAsync();
        Task<bool> CreateUserAsync(string username, string fullName, string email, string mobileNumber, string password, string role);
        Task<List<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> UpdateUserAsync(User user, string newUsername, string newFullName, string newEmail, string newMobileNumber, string newPassword, string newRole);
        Task<User?> GetUserByEmailOrMobileAsync(string emailOrMobile);
    }
}