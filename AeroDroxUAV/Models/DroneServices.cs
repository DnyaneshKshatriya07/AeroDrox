namespace AeroDroxUAV.Models
{
    public class DroneServices
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required string Category { get; set; }
        public string? Benefits { get; set; }
        public string? ImageUrl { get; set; }
        public string? VideoUrl { get; set; } // Added for video support
        public required string Duration { get; set; } // Added duration field
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}