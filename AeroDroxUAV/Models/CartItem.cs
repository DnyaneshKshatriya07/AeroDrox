using System.ComponentModel.DataAnnotations.Schema;

namespace AeroDroxUAV.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? DroneId { get; set; }
        public int? AccessoryId { get; set; }
        public int Quantity { get; set; } = 1;
        public DateTime AddedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        
        [ForeignKey("DroneId")]
        public virtual Drone? Drone { get; set; }
        
        [ForeignKey("AccessoryId")]
        public virtual Accessories? Accessory { get; set; }

        // Helper properties
        [NotMapped]
        public string ProductType => DroneId.HasValue ? "Drone" : "Accessory";
        
        [NotMapped]
        public string ProductName => DroneId.HasValue ? Drone?.Name ?? "Unknown" : Accessory?.Name ?? "Unknown";
        
        [NotMapped]
        public double ProductPrice
        {
            get
            {
                if (DroneId.HasValue && Drone != null)
                    return Drone.DiscountPrice ?? Drone.Price;
                if (AccessoryId.HasValue && Accessory != null)
                    return Accessory.DiscountPrice ?? Accessory.Price;
                return 0;
            }
        }
        
        [NotMapped]
        public string? ProductImage => DroneId.HasValue ? Drone?.ImageUrl : Accessory?.ImageUrl;
    }
}