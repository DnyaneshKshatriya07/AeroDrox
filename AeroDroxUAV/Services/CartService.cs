namespace AeroDroxUAV.Services
{
    using AeroDroxUAV.Models;
    using AeroDroxUAV.Repositories;

    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IDroneRepository _droneRepository;
        private readonly IAccessoriesRepository _accessoriesRepository;

        public CartService(
            ICartRepository cartRepository,
            IDroneRepository droneRepository,
            IAccessoriesRepository accessoriesRepository)
        {
            _cartRepository = cartRepository;
            _droneRepository = droneRepository;
            _accessoriesRepository = accessoriesRepository;
        }

        public async Task<IEnumerable<CartItem>> GetUserCartAsync(int userId)
        {
            return await _cartRepository.GetUserCartAsync(userId);
        }

        public async Task<CartItem?> GetCartItemAsync(int id)
        {
            return await _cartRepository.GetCartItemAsync(id);
        }

        public async Task AddToCartAsync(int userId, int? droneId, int? accessoryId, int quantity = 1)
        {
            // Validate product exists
            if (droneId.HasValue)
            {
                var drone = await _droneRepository.GetByIdAsync(droneId.Value);
                if (drone == null)
                    throw new ArgumentException("Drone not found");
                
                if (drone.StockQuantity < quantity)
                    throw new ArgumentException("Not enough stock available");
            }
            else if (accessoryId.HasValue)
            {
                var accessory = await _accessoriesRepository.GetByIdAsync(accessoryId.Value);
                if (accessory == null)
                    throw new ArgumentException("Accessory not found");
                
                if (accessory.StockQuantity < quantity)
                    throw new ArgumentException("Not enough stock available");
            }
            else
            {
                throw new ArgumentException("Either droneId or accessoryId must be provided");
            }

            // Check if item already exists in cart
            var existingItem = await _cartRepository.GetCartItemByProductAsync(userId, droneId, accessoryId);
            
            if (existingItem != null)
            {
                // Update quantity
                existingItem.Quantity += quantity;
                _cartRepository.UpdateCartItem(existingItem);
            }
            else
            {
                // Add new item
                var cartItem = new CartItem
                {
                    UserId = userId,
                    DroneId = droneId,
                    AccessoryId = accessoryId,
                    Quantity = quantity,
                    AddedAt = DateTime.Now
                };
                await _cartRepository.AddToCartAsync(cartItem);
            }

            await _cartRepository.SaveChangesAsync();
        }

        public async Task UpdateQuantityAsync(int cartItemId, int quantity)
        {
            var cartItem = await _cartRepository.GetCartItemAsync(cartItemId);
            if (cartItem == null)
                throw new ArgumentException("Cart item not found");

            if (quantity <= 0)
            {
                // Remove item if quantity is 0 or negative
                _cartRepository.RemoveFromCart(cartItem);
            }
            else
            {
                // Check stock availability
                if (cartItem.DroneId.HasValue && cartItem.Drone != null)
                {
                    if (cartItem.Drone.StockQuantity < quantity)
                        throw new ArgumentException("Not enough stock available");
                }
                else if (cartItem.AccessoryId.HasValue && cartItem.Accessory != null)
                {
                    if (cartItem.Accessory.StockQuantity < quantity)
                        throw new ArgumentException("Not enough stock available");
                }

                cartItem.Quantity = quantity;
                _cartRepository.UpdateCartItem(cartItem);
            }

            await _cartRepository.SaveChangesAsync();
        }

        public async Task RemoveFromCartAsync(int cartItemId)
        {
            var cartItem = await _cartRepository.GetCartItemAsync(cartItemId);
            if (cartItem != null)
            {
                _cartRepository.RemoveFromCart(cartItem);
                await _cartRepository.SaveChangesAsync();
            }
        }

        public async Task ClearCartAsync(int userId)
        {
            await _cartRepository.RemoveAllFromCartAsync(userId);
            await _cartRepository.SaveChangesAsync();
        }

        public async Task<int> GetCartItemCountAsync(int userId)
        {
            return await _cartRepository.GetCartItemCountAsync(userId);
        }

        public async Task<double> GetCartTotalAsync(int userId)
        {
            var cartItems = await _cartRepository.GetUserCartAsync(userId);
            return cartItems.Sum(item => item.ProductPrice * item.Quantity);
        }
    }
}