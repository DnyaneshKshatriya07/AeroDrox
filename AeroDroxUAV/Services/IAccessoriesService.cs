namespace AeroDroxUAV.Services
{
    using AeroDroxUAV.Models;
    
    public interface IAccessoriesService
    {
        Task<IEnumerable<Accessories>> GetAllAccessoriesAsync(); // Corrected return type
        Task<Accessories?> GetAccessoriesByIdAsync(int id);
        Task CreateAccessoriesAsync(Accessories accessories);
        Task UpdateAccessoriesAsync(Accessories accessories);
        Task DeleteAccessoriesAsync(int id);
    }
}