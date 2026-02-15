namespace AeroDroxUAV.Services
{
    using AeroDroxUAV.Models;
    
    public interface ICartService
    {
        Task<IEnumerable<CartItem>> GetUserCartAsync(int userId);
        Task<CartItem?> GetCartItemAsync(int id);
        Task AddToCartAsync(int userId, int? droneId, int? accessoryId, int quantity = 1);
        Task UpdateQuantityAsync(int cartItemId, int quantity);
        Task RemoveFromCartAsync(int cartItemId);
        Task ClearCartAsync(int userId);
        Task<int> GetCartItemCountAsync(int userId);
        Task<double> GetCartTotalAsync(int userId);
    }
}