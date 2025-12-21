namespace AeroDroxUAV.Models
{
    public class DroneServices
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Type { get; set; }
        public double PricePerAcre { get; set; }
        public string? Description { get; set; }
        public required string Category { get; set; }
        public string? Benefits { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}