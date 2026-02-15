namespace AeroDroxUAV.Repositories
{
    using AeroDroxUAV.Models;
    
    public interface ICartRepository
    {
        Task<IEnumerable<CartItem>> GetUserCartAsync(int userId);
        Task<CartItem?> GetCartItemAsync(int id);
        Task<CartItem?> GetCartItemByProductAsync(int userId, int? droneId, int? accessoryId);
        Task AddToCartAsync(CartItem cartItem);
        void UpdateCartItem(CartItem cartItem);
        void RemoveFromCart(CartItem cartItem);
        Task RemoveAllFromCartAsync(int userId);
        Task<int> GetCartItemCountAsync(int userId);
        Task SaveChangesAsync();
    }
}