using System.ComponentModel.DataAnnotations.Schema;

namespace AeroDroxUAV.Models
{
    public class Drone
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public double Price { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? Condition { get; set; } // New field: New, Refurbished, Used
        public int StockQuantity { get; set; } = 0;
        public bool IsFeatured { get; set; } = false;
        public bool ShowOnHomepage { get; set; } = false; // NEW FIELD: Add to Home Page
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public double? DiscountPrice { get; set; } // New field: Optional discount price
        
        // Image properties
        public string? ImageUrl { get; set; }
        [NotMapped]
        public IFormFile? ImageFile { get; set; }
    }
}