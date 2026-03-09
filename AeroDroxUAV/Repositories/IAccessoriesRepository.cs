namespace AeroDroxUAV.Repositories
{
    using AeroDroxUAV.Models;
    
    public interface IAccessoriesRepository
    {
        Task<IEnumerable<Accessories>> GetAllAsync();
        Task<Accessories?> GetByIdAsync(int id);
        Task<Accessories?> GetByIdForOrderAsync(int id); // New method without tracking
        Task<Accessories?> GetByIdForUpdateAsync(int id); // New method with tracking
        Task AddAsync(Accessories accessories);
        void Update(Accessories accessories);
        Task UpdateAndSaveAsync(Accessories accessories); // New method
        void Delete(Accessories accessories);
        Task SaveChangesAsync();
        
        // NEW METHOD: Get accessories for homepage
        Task<IEnumerable<Accessories>> GetHomepageAccessoriesAsync();
    }
}