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
        public DbSet<CartItem> CartItems { get; set; } // NEW: Cart items table

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships for CartItem
            modelBuilder.Entity<CartItem>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(c => c.Drone)
                .WithMany()
                .HasForeignKey(c => c.DroneId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<CartItem>()
                .HasOne(c => c.Accessory)
                .WithMany()
                .HasForeignKey(c => c.AccessoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // Ensure a user can't have duplicate items (same drone/accessory)
            modelBuilder.Entity<CartItem>()
                .HasIndex(c => new { c.UserId, c.DroneId })
                .IsUnique()
                .HasFilter("[DroneId] IS NOT NULL");

            modelBuilder.Entity<CartItem>()
                .HasIndex(c => new { c.UserId, c.AccessoryId })
                .IsUnique()
                .HasFilter("[AccessoryId] IS NOT NULL");

            // Fix the shadow property issue
            modelBuilder.Entity<CartItem>()
                .Property(c => c.UserId)
                .HasColumnName("UserId");
        }
    }
}