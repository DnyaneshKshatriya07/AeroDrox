using System.ComponentModel.DataAnnotations.Schema;

namespace AeroDroxUAV.Models
{
    public class Drone
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Type { get; set; }
        public double Price { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? BatteryLife { get; set; }
        public double? Range { get; set; }
        public int StockQuantity { get; set; } = 0;
        public bool IsFeatured { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Image properties
        public string? ImageUrl { get; set; }
        [NotMapped]
        public IFormFile? ImageFile { get; set; }
    }
}