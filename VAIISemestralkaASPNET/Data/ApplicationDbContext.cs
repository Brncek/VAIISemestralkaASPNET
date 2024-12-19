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

        public DbSet<Car> Car { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<ClosedDate> ClosedDates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // CHANGE FOR SqLite -> ChatGPT
            modelBuilder.Entity<IdentityRole>(entity =>
            {
                entity.Property(r => r.Id).HasColumnType("TEXT");
                entity.Property(r => r.Name).HasColumnType("TEXT").HasMaxLength(256);
                entity.Property(r => r.NormalizedName).HasColumnType("TEXT").HasMaxLength(256);
                entity.Property(r => r.ConcurrencyStamp).HasColumnType("TEXT");
            });

            modelBuilder.Entity<IdentityUser>(entity =>
            {
                entity.Property(u => u.Id).HasColumnType("TEXT");
                entity.Property(u => u.UserName).HasColumnType("TEXT").HasMaxLength(256);
                entity.Property(u => u.NormalizedUserName).HasColumnType("TEXT").HasMaxLength(256);
                entity.Property(u => u.Email).HasColumnType("TEXT").HasMaxLength(256);
                entity.Property(u => u.NormalizedEmail).HasColumnType("TEXT").HasMaxLength(256);
                entity.Property(u => u.ConcurrencyStamp).HasColumnType("TEXT");
                entity.Property(u => u.SecurityStamp).HasColumnType("TEXT");
            });

            modelBuilder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.Property(ur => ur.UserId).HasColumnType("TEXT");
                entity.Property(ur => ur.RoleId).HasColumnType("TEXT");
            });

            modelBuilder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.Property(ul => ul.LoginProvider).HasColumnType("TEXT");
                entity.Property(ul => ul.ProviderKey).HasColumnType("TEXT");
                entity.Property(ul => ul.UserId).HasColumnType("TEXT");
            });

            modelBuilder.Entity<IdentityUserToken<string>>(entity =>
            {
                entity.Property(ut => ut.UserId).HasColumnType("TEXT");
                entity.Property(ut => ut.LoginProvider).HasColumnType("TEXT");
                entity.Property(ut => ut.Name).HasColumnType("TEXT");
            });

            modelBuilder.Entity<IdentityRoleClaim<string>>(entity =>
            {
                entity.Property(rc => rc.Id).HasColumnType("INTEGER"); 
                entity.Property(rc => rc.RoleId).HasColumnType("TEXT");
                entity.Property(rc => rc.ClaimType).HasColumnType("TEXT");
                entity.Property(rc => rc.ClaimValue).HasColumnType("TEXT");
            });

            modelBuilder.Entity<IdentityUserClaim<string>>(entity =>
            {
                entity.Property(uc => uc.Id).HasColumnType("INTEGER");
                entity.Property(uc => uc.UserId).HasColumnType("TEXT");
                entity.Property(uc => uc.ClaimType).HasColumnType("TEXT");
                entity.Property(uc => uc.ClaimValue).HasColumnType("TEXT");
            });

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entity.GetProperties())
                {
                    if (property.GetColumnType() == "nvarchar(max)")
                    {
                        property.SetColumnType("TEXT");
                    }
                    else if (property.ClrType == typeof(DateTime))
                    {
                        property.SetColumnType("TEXT"); 
                    }
                    else if (property.ClrType == typeof(DateTimeOffset))
                    {
                        property.SetColumnType("TEXT"); 
                    }
                }
            }
            //------------------------------------------------------

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

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Service)
                .WithMany()
                .HasForeignKey(o => o.ServiceId)
                .IsRequired(false); 
        }
    }
}
