namespace AeroDroxUAV.Services
{
    using AeroDroxUAV.Models;
    using AeroDroxUAV.Repositories;
    
    public class AccessoriesService : IAccessoriesService
    {
        private readonly IAccessoriesRepository _accessoriesRepository;

        public AccessoriesService(IAccessoriesRepository accessoriesRepository)
        {
            _accessoriesRepository = accessoriesRepository;
        }

        public async Task<IEnumerable<Accessories>> GetAllAccessoriesAsync()
        {
            return await _accessoriesRepository.GetAllAsync();
        }

        public async Task<Accessories?> GetAccessoriesByIdAsync(int id)
        {
            return await _accessoriesRepository.GetByIdAsync(id);
        }

        public async Task CreateAccessoriesAsync(Accessories accessories)
        {
            await _accessoriesRepository.AddAsync(accessories);
            await _accessoriesRepository.SaveChangesAsync();
        }

        public async Task UpdateAccessoriesAsync(Accessories accessories)
        {
            _accessoriesRepository.Update(accessories);
            await _accessoriesRepository.SaveChangesAsync();
        }

        public async Task DeleteAccessoriesAsync(int id)
        {
            var accessories = await _accessoriesRepository.GetByIdAsync(id);
            if (accessories != null)
            {
                _accessoriesRepository.Delete(accessories);
                await _accessoriesRepository.SaveChangesAsync();
            }
        }
    }
}