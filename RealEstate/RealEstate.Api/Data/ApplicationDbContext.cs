using Microsoft.EntityFrameworkCore;
using RealEstate.Api.Entities;
using RealEstate.Api.Models;

namespace RealEstate.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Listing> Listings { get; set; } = null!;
        public DbSet<Property> Properties { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User entity konfigürasyonu
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Role).HasMaxLength(50).HasDefaultValue("User");
            });

            // Listing entity konfigürasyonu
            modelBuilder.Entity<Listing>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(200);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.District).HasMaxLength(100);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.DeactivationReason).HasMaxLength(500);

                entity.HasOne(e => e.Owner)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.DeactivatedBy)
                    .WithMany()
                    .HasForeignKey(e => e.DeactivatedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Notification entity konfigürasyonu
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Message).HasMaxLength(1000).IsRequired();
                entity.Property(e => e.Type).HasMaxLength(50).HasDefaultValue("info");

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Listing)
                    .WithMany()
                    .HasForeignKey(e => e.ListingId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Property entity konfigürasyonu
            modelBuilder.Entity<Property>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
                entity.Property(e => e.City).HasMaxLength(100).IsRequired();
                entity.Property(e => e.District).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ImageUrl).HasMaxLength(500).IsRequired();
            });

            // Seed data - başlangıç verileri
            modelBuilder.Entity<Property>().HasData(
                new Property
                {
                    Id = 1,
                    Title = "Merkezde 2+1 Daire",
                    City = "Manisa",
                    District = "Yunusemre",
                    Price = 2_500_000,
                    RoomCount = 2,
                    Area = 110,
                    ImageUrl = "https://placehold.co/600x400?text=House+1"
                },
                new Property
                {
                    Id = 2,
                    Title = "Site İçinde 3+1 Daire",
                    City = "İzmir",
                    District = "Bornova",
                    Price = 4_100_000,
                    RoomCount = 3,
                    Area = 140,
                    ImageUrl = "https://placehold.co/600x400?text=House+2"
                }
            );
        }
    }
}
