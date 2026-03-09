namespace AeroDroxUAV.Repositories
{
    using AeroDroxUAV.Data;
    using AeroDroxUAV.Models;
    using Microsoft.EntityFrameworkCore;

    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            
            Console.WriteLine($"GetAllOrdersAsync: Found {orders.Count} orders");
            return orders;
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(int userId)
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            
            Console.WriteLine($"GetUserOrdersAsync for user {userId}: Found {orders.Count} orders");
            return orders;
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Order?> GetOrderWithItemsAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Drone)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Accessory)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task AddOrderAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
        }

        public void UpdateOrder(Order order)
        {
            _context.Orders.Update(order);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(string status)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .Where(o => o.OrderStatus == status)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<int> GetOrderCountAsync()
        {
            return await _context.Orders.CountAsync();
        }

        public async Task<double> GetTotalRevenueAsync()
        {
            return await _context.Orders
                .Where(o => o.PaymentStatus == "Completed")
                .SumAsync(o => o.TotalAmount);
        }

        public async Task<Dictionary<string, int>> GetOrderStatusStatsAsync()
        {
            var stats = new Dictionary<string, int>();
            
            stats["Pending"] = await _context.Orders.CountAsync(o => o.OrderStatus == "Pending");
            stats["Confirmed"] = await _context.Orders.CountAsync(o => o.OrderStatus == "Confirmed");
            stats["Shipped"] = await _context.Orders.CountAsync(o => o.OrderStatus == "Shipped");
            stats["Delivered"] = await _context.Orders.CountAsync(o => o.OrderStatus == "Delivered");
            stats["Cancelled"] = await _context.Orders.CountAsync(o => o.OrderStatus == "Cancelled");
            
            return stats;
        }
    }
}