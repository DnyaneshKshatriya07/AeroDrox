namespace AeroDroxUAV.Repositories
{
    using AeroDroxUAV.Models;
    
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<IEnumerable<Order>> GetUserOrdersAsync(int userId);
        Task<Order?> GetOrderByIdAsync(int id);
        Task<Order?> GetOrderWithItemsAsync(int id);
        Task AddOrderAsync(Order order);
        void UpdateOrder(Order order);
        Task SaveChangesAsync();
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(string status);
        Task<int> GetOrderCountAsync();
        Task<double> GetTotalRevenueAsync();
        Task<Dictionary<string, int>> GetOrderStatusStatsAsync();
    }
}