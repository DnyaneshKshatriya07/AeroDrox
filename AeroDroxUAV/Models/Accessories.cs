namespace AeroDroxUAV.Models
{
    public class Accessories
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Type { get; set; }
        public double Price { get; set; }
        public string? Description { get; set; }
    }
}