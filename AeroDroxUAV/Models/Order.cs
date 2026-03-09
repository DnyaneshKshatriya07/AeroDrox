namespace AeroDroxUAV.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public double TotalAmount { get; set; }
        public string OrderStatus { get; set; } = "Pending"; // Pending, Confirmed, Shipped, Delivered, Cancelled
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Completed, Failed, Refunded
        public string PaymentMethod { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        
        // Shipping Information
        public string ShippingAddress { get; set; } = string.Empty;
        public string ShippingCity { get; set; } = string.Empty;
        public string ShippingState { get; set; } = string.Empty;
        public string ShippingPinCode { get; set; } = string.Empty;
        public string? ShippingPhone { get; set; }
        
        // Navigation property
        public virtual User? User { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}