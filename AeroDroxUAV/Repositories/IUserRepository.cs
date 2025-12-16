namespace AeroDroxUAV.Repositories
{
    using AeroDroxUAV.Models;
    
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAndPasswordAsync(string username, string password);
        Task<User?> GetByEmailOrMobileAndPasswordAsync(string emailOrMobile, string password);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByMobileNumberAsync(string mobileNumber);
        Task<bool> HasUsersAsync();
        Task AddAsync(User user);
        Task SaveChangesAsync();
        Task<List<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task DeleteUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task<User?> GetByEmailOrMobileAsync(string emailOrMobile);
    }
}