namespace AeroDroxUAV.Repositories
{
    using AeroDroxUAV.Models;
    
    public interface IAccessoriesRepository
    {
        Task<IEnumerable<Accessories>> GetAllAsync();
        Task<Accessories?> GetByIdAsync(int id);
        Task AddAsync(Accessories accessories);
        void Update(Accessories accessories); // Correct return type and name
        void Delete(Accessories accessories);
        Task SaveChangesAsync();
    }
}