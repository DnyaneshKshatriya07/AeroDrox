namespace AeroDroxUAV.Repositories
{
    using AeroDroxUAV.Models;
    
    public interface IDroneRepository
    {
        Task<IEnumerable<Drone>> GetAllAsync();
        Task<Drone?> GetByIdAsync(int id);
        Task<Drone?> GetByIdForOrderAsync(int id); // New method without tracking
        Task<Drone?> GetByIdForUpdateAsync(int id); // New method with tracking
        Task AddAsync(Drone drone);
        void Update(Drone drone);
        Task UpdateAndSaveAsync(Drone drone); // New method
        void Delete(Drone drone);
        Task SaveChangesAsync();
        
        // Optional methods
        Task<IEnumerable<Drone>> GetByCategoryAsync(string category);
        Task<IEnumerable<Drone>> GetFeaturedAsync();
        Task<IEnumerable<Drone>> GetHomepageDronesAsync();
    }
}