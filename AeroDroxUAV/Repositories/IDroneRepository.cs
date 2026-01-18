namespace AeroDroxUAV.Repositories
{
    using AeroDroxUAV.Models;
    
    public interface IDroneRepository
    {
        Task<IEnumerable<Drone>> GetAllAsync();
        Task<Drone?> GetByIdAsync(int id);
        Task AddAsync(Drone drone);
        void Update(Drone drone);
        void Delete(Drone drone);
        Task SaveChangesAsync();
        
        // Optional methods
        Task<IEnumerable<Drone>> GetByCategoryAsync(string category);
        Task<IEnumerable<Drone>> GetFeaturedAsync();
        Task<IEnumerable<Drone>> GetHomepageDronesAsync(); // NEW METHOD
    }
}