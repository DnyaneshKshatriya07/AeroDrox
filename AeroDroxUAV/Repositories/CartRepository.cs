namespace AeroDroxUAV.Repositories
{
    using AeroDroxUAV.Data;
    using AeroDroxUAV.Models;
    using Microsoft.EntityFrameworkCore;

    public class CartRepository : ICartRepository
    {
        private readonly AppDbContext _context;

        public CartRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CartItem>> GetUserCartAsync(int userId)
        {
            return await _context.CartItems
                .Include(c => c.Drone)
                .Include(c => c.Accessory)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.AddedAt)
                .ToListAsync();
        }

        public async Task<CartItem?> GetCartItemAsync(int id)
        {
            return await _context.CartItems
                .Include(c => c.Drone)
                .Include(c => c.Accessory)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<CartItem?> GetCartItemByProductAsync(int userId, int? droneId, int? accessoryId)
        {
            return await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && 
                    ((droneId.HasValue && c.DroneId == droneId) || 
                     (accessoryId.HasValue && c.AccessoryId == accessoryId)));
        }

        public async Task AddToCartAsync(CartItem cartItem)
        {
            await _context.CartItems.AddAsync(cartItem);
        }

        public void UpdateCartItem(CartItem cartItem)
        {
            _context.CartItems.Update(cartItem);
        }

        public void RemoveFromCart(CartItem cartItem)
        {
            _context.CartItems.Remove(cartItem);
        }

        public async Task RemoveAllFromCartAsync(int userId)
        {
            var items = await _context.CartItems
                .Where(c => c.UserId == userId)
                .ToListAsync();
            
            _context.CartItems.RemoveRange(items);
        }

        public async Task<int> GetCartItemCountAsync(int userId)
        {
            return await _context.CartItems
                .Where(c => c.UserId == userId)
                .SumAsync(c => c.Quantity);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}