namespace AeroDroxUAV.Services
{
    using AeroDroxUAV.Models;
    
    public interface IDroneService
    {
        Task<IEnumerable<Drone>> GetAllDronesAsync();
        Task<Drone?> GetDroneByIdAsync(int id);
        Task CreateDroneAsync(Drone drone);
        Task UpdateDroneAsync(Drone drone);
        Task DeleteDroneAsync(int id);
        
        // Optional methods you might need:
        Task<IEnumerable<Drone>> GetFeaturedDronesAsync();
        Task<IEnumerable<Drone>> GetDronesByCategoryAsync(string category);
        Task UpdateStockQuantityAsync(int droneId, int quantity);
        Task<IEnumerable<Drone>> GetHomepageDronesAsync(); // NEW METHOD
    }
}