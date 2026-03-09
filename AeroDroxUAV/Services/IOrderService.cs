namespace AeroDroxUAV.Services
{
    using AeroDroxUAV.Models;
    
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<IEnumerable<Order>> GetUserOrdersAsync(int userId);
        Task<Order?> GetOrderByIdAsync(int id);
        Task<Order?> GetOrderWithDetailsAsync(int id);
        Task<bool> CreateOrderAsync(int userId, PaymentViewModel payment, List<CartItem> cartItems);
        Task<bool> UpdateOrderStatusAsync(int orderId, string orderStatus, string? paymentStatus = null);
        Task<bool> CancelOrderAsync(int orderId, int userId);
        Task<int> GetOrderCountAsync();
        Task<double> GetTotalRevenueAsync();
        Task<Dictionary<string, int>> GetOrderStatusStatsAsync();
        Task<List<Order>> GetRecentOrdersAsync(int count = 10);
    }
}