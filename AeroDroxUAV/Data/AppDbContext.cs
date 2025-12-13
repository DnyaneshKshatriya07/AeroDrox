using Microsoft.EntityFrameworkCore;
using AeroDroxUAV.Models;

namespace AeroDroxUAV.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

        public DbSet<User> Users { get; set; }
        public DbSet<Drone> Drones { get; set; }
        public DbSet<DroneServices> DroneServices {get; set;}
        public DbSet<Accessories> Accessories {get; set;}
    }
}
