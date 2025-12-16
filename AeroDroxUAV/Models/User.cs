namespace AeroDroxUAV.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string MobileNumber { get; set; }
        public required string Password { get; set; }
        public required string Role { get; set; } // "Admin" or "User"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
    }
}