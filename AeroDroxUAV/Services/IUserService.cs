namespace AeroDroxUAV.Services
{
    using AeroDroxUAV.Models;

    public interface IUserService
    {
        Task<User?> AuthenticateAsync(string username, string password);
        Task SeedDefaultUsersAsync();
        Task<bool> CreateUserAsync(string username, string password, string role); 
        // NEW: Method to get all users
        Task<List<User>> GetAllUsersAsync();
        // NEW: Methods for Edit/Delete
        Task<User?> GetUserByIdAsync(int id);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> UpdateUserAsync(User user, string newUsername, string newPassword, string newRole);
    }
}