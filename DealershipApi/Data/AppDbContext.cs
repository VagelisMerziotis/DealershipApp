using Microsoft.EntityFrameworkCore;
using DealershipApi.Models;

namespace DealershipApi.Data;

public class AppDbContext : DbContext
{
    // Constructor
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}
    
    public DbSet<User> Users { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Dealership> Dealerships { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Vehicle>()
            .HasDiscriminator<string>("VehicleType")
            .HasValue<Automobile>("Automobile");
        
        //Create foreign key and cascade policy with dealership 
        modelBuilder.Entity<Vehicle>()
            .HasOne(v => v.Dealership)
            .WithMany(d => d.Vehicles)
            .HasForeignKey(v => v.DealershipId)
            .OnDelete(DeleteBehavior.Cascade);

        // Create foreign key and cascade with dealership
        modelBuilder.Entity<User>()
            .HasOne(u => u.Dealership)
            .WithMany(d => d.Users)
            .HasForeignKey(u => u.DealershipId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}