namespace AeroDroxUAV.Services
{
    using AeroDroxUAV.Models;
    using AeroDroxUAV.Repositories;
    using Microsoft.EntityFrameworkCore;
    
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IDroneRepository _droneRepository;
        private readonly IAccessoriesRepository _accessoriesRepository;

        public OrderService(
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            IDroneRepository droneRepository,
            IAccessoriesRepository accessoriesRepository)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _droneRepository = droneRepository;
            _accessoriesRepository = accessoriesRepository;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _orderRepository.GetAllOrdersAsync();
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(int userId)
        {
            return await _orderRepository.GetUserOrdersAsync(userId);
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            return await _orderRepository.GetOrderByIdAsync(id);
        }

        public async Task<Order?> GetOrderWithDetailsAsync(int id)
        {
            return await _orderRepository.GetOrderWithItemsAsync(id);
        }

        public async Task<bool> CreateOrderAsync(int userId, PaymentViewModel payment, List<CartItem> cartItems)
        {
            if (!cartItems.Any())
            {
                Console.WriteLine("CreateOrderAsync: Cart is empty");
                return false;
            }

            try
            {
                // Set payment status based on method
                string paymentStatus = payment.PaymentMethod == "COD" ? "Pending" : "Completed";
                string paymentMethod = payment.PaymentMethod == "COD" ? "Cash on Delivery" : payment.PaymentMethod;
                
                // Calculate total amount
                double totalAmount = 0;
                
                // Create order
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.Now,
                    OrderStatus = "Pending",
                    PaymentStatus = paymentStatus,
                    PaymentMethod = paymentMethod,
                    TransactionId = GenerateTransactionId(),
                    ShippingAddress = payment.ShippingAddress?.Trim() ?? "",
                    ShippingCity = payment.ShippingCity?.Trim() ?? "",
                    ShippingState = payment.ShippingState?.Trim() ?? "",
                    ShippingPinCode = payment.ShippingPinCode?.Trim() ?? "",
                    ShippingPhone = payment.ShippingPhone?.Trim() ?? "",
                    OrderItems = new List<OrderItem>()
                };

                Console.WriteLine($"Creating order for user {userId} with {cartItems.Count} items");

                // Add order items
                foreach (var cartItem in cartItems)
                {
                    // Get fresh product data from database WITHOUT tracking
                    if (cartItem.DroneId.HasValue)
                    {
                        var drone = await _droneRepository.GetByIdForOrderAsync(cartItem.DroneId.Value);
                        if (drone == null) 
                        {
                            Console.WriteLine($"Drone {cartItem.DroneId} not found");
                            continue;
                        }
                        
                        double unitPrice = drone.Price;
                        double discountPrice = drone.DiscountPrice ?? drone.Price;
                        string productName = drone.Name;
                        string productType = "Drone";
                        
                        totalAmount += discountPrice * cartItem.Quantity;

                        var orderItem = new OrderItem
                        {
                            DroneId = cartItem.DroneId,
                            Quantity = cartItem.Quantity,
                            UnitPrice = unitPrice,
                            DiscountPrice = discountPrice,
                            ProductName = productName,
                            ProductType = productType
                        };
                        
                        order.OrderItems.Add(orderItem);
                        Console.WriteLine($"Added drone: {productName}, Qty: {cartItem.Quantity}, Price: {discountPrice}");

                        // Update stock quantity - need a separate method that doesn't conflict with tracking
                        await UpdateDroneStockAsync(cartItem.DroneId.Value, cartItem.Quantity);
                    }
                    else if (cartItem.AccessoryId.HasValue)
                    {
                        var accessory = await _accessoriesRepository.GetByIdForOrderAsync(cartItem.AccessoryId.Value);
                        if (accessory == null) 
                        {
                            Console.WriteLine($"Accessory {cartItem.AccessoryId} not found");
                            continue;
                        }
                        
                        double unitPrice = accessory.Price;
                        double discountPrice = accessory.DiscountPrice ?? accessory.Price;
                        string productName = accessory.Name;
                        string productType = "Accessory";
                        
                        totalAmount += discountPrice * cartItem.Quantity;

                        var orderItem = new OrderItem
                        {
                            AccessoryId = cartItem.AccessoryId,
                            Quantity = cartItem.Quantity,
                            UnitPrice = unitPrice,
                            DiscountPrice = discountPrice,
                            ProductName = productName,
                            ProductType = productType
                        };
                        
                        order.OrderItems.Add(orderItem);
                        Console.WriteLine($"Added accessory: {productName}, Qty: {cartItem.Quantity}, Price: {discountPrice}");

                        // Update stock quantity
                        await UpdateAccessoryStockAsync(cartItem.AccessoryId.Value, cartItem.Quantity);
                    }
                }

                // Set total amount
                order.TotalAmount = totalAmount;
                Console.WriteLine($"Order total amount: {totalAmount}");

                // Save order
                await _orderRepository.AddOrderAsync(order);
                await _orderRepository.SaveChangesAsync();
                Console.WriteLine($"Order saved with ID: {order.Id}");

                // Clear cart
                await _cartRepository.RemoveAllFromCartAsync(userId);
                await _cartRepository.SaveChangesAsync();
                Console.WriteLine("Cart cleared");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating order: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return false;
            }
        }

        private async Task UpdateDroneStockAsync(int droneId, int quantity)
        {
            try
            {
                // Get the drone directly from context with tracking for update
                var drone = await _droneRepository.GetByIdForUpdateAsync(droneId);
                if (drone != null)
                {
                    drone.StockQuantity -= quantity;
                    await _droneRepository.UpdateAndSaveAsync(drone);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating drone stock: {ex.Message}");
            }
        }

        private async Task UpdateAccessoryStockAsync(int accessoryId, int quantity)
        {
            try
            {
                // Get the accessory directly from context with tracking for update
                var accessory = await _accessoriesRepository.GetByIdForUpdateAsync(accessoryId);
                if (accessory != null)
                {
                    accessory.StockQuantity -= quantity;
                    await _accessoriesRepository.UpdateAndSaveAsync(accessory);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating accessory stock: {ex.Message}");
            }
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string orderStatus, string? paymentStatus = null)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
                return false;

            order.OrderStatus = orderStatus;
            if (!string.IsNullOrEmpty(paymentStatus))
            {
                order.PaymentStatus = paymentStatus;
            }

            _orderRepository.UpdateOrder(order);
            await _orderRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelOrderAsync(int orderId, int userId)
        {
            var order = await _orderRepository.GetOrderWithItemsAsync(orderId);
            if (order == null || order.UserId != userId)
                return false;

            if (order.OrderStatus == "Pending" || order.OrderStatus == "Confirmed")
            {
                order.OrderStatus = "Cancelled";
                
                // Restore stock quantities
                foreach (var item in order.OrderItems)
                {
                    if (item.DroneId.HasValue)
                    {
                        await UpdateDroneStockForCancelAsync(item.DroneId.Value, item.Quantity);
                    }
                    else if (item.AccessoryId.HasValue)
                    {
                        await UpdateAccessoryStockForCancelAsync(item.AccessoryId.Value, item.Quantity);
                    }
                }

                _orderRepository.UpdateOrder(order);
                await _orderRepository.SaveChangesAsync();
                return true;
            }

            return false;
        }

        private async Task UpdateDroneStockForCancelAsync(int droneId, int quantity)
        {
            try
            {
                var drone = await _droneRepository.GetByIdForUpdateAsync(droneId);
                if (drone != null)
                {
                    drone.StockQuantity += quantity;
                    await _droneRepository.UpdateAndSaveAsync(drone);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating drone stock on cancel: {ex.Message}");
            }
        }

        private async Task UpdateAccessoryStockForCancelAsync(int accessoryId, int quantity)
        {
            try
            {
                var accessory = await _accessoriesRepository.GetByIdForUpdateAsync(accessoryId);
                if (accessory != null)
                {
                    accessory.StockQuantity += quantity;
                    await _accessoriesRepository.UpdateAndSaveAsync(accessory);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating accessory stock on cancel: {ex.Message}");
            }
        }

        public async Task<int> GetOrderCountAsync()
        {
            return await _orderRepository.GetOrderCountAsync();
        }

        public async Task<double> GetTotalRevenueAsync()
        {
            return await _orderRepository.GetTotalRevenueAsync();
        }

        public async Task<Dictionary<string, int>> GetOrderStatusStatsAsync()
        {
            return await _orderRepository.GetOrderStatusStatsAsync();
        }

        public async Task<List<Order>> GetRecentOrdersAsync(int count = 10)
        {
            var orders = await _orderRepository.GetAllOrdersAsync();
            return orders.Take(count).ToList();
        }

        private string GenerateTransactionId()
        {
            return "TXN" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(1000, 9999).ToString();
        }
    }
}