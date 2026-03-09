using System.ComponentModel.DataAnnotations.Schema;

namespace AeroDroxUAV.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int? DroneId { get; set; }
        public int? AccessoryId { get; set; }
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public double DiscountPrice { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductType { get; set; } = string.Empty; // "Drone" or "Accessory"
        
        // Navigation properties
        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }
        
        [ForeignKey("DroneId")]
        public virtual Drone? Drone { get; set; }
        
        [ForeignKey("AccessoryId")]
        public virtual Accessories? Accessory { get; set; }
        
        [NotMapped]
        public double TotalPrice => Quantity * DiscountPrice;
    }
}