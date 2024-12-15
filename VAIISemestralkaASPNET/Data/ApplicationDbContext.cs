using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VAIISemestralkaASPNET.Models;

namespace VAIISemestralkaASPNET.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet for all models
        public DbSet<Car> Car { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ClosedDate> ClosedDates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Car)
                .WithMany()
                .HasForeignKey(o => o.CarID)
                .IsRequired(false); 

            modelBuilder.Entity<Service>()
                .HasOne(s => s.Car)
                .WithMany()
                .HasForeignKey(s => s.CarID)
                .IsRequired(false); 

            modelBuilder.Entity<Service>()
                .HasOne(s => s.Order)
                .WithMany()
                .HasForeignKey(s => s.OrderId)
                .IsRequired(true); 
        }
    }
}
