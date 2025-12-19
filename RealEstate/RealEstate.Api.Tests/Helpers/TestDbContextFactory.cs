using Microsoft.EntityFrameworkCore;
using RealEstate.Api.Data;
using RealEstate.Api.Models;
using RealEstate.Api.Entities;

namespace RealEstate.Api.Tests.Helpers
{
    /// <summary>
    /// Test için In-Memory veritabanı oluşturan yardımcı sınıf
    /// </summary>
    public static class TestDbContextFactory
    {
        /// <summary>
        /// Her test için benzersiz bir In-Memory veritabanı oluşturur
        /// </summary>
        public static ApplicationDbContext CreateContext(string? databaseName = null)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            return context;
        }

        /// <summary>
        /// Seed data ile birlikte context oluşturur
        /// </summary>
        public static ApplicationDbContext CreateContextWithData(string? databaseName = null)
        {
            var context = CreateContext(databaseName);

            // Seed data ekle
            if (!context.Properties.Any())
            {
                context.Properties.AddRange(
                    new Property
                    {
                        Id = 1,
                        Title = "Test Daire 1",
                        City = "İstanbul",
                        District = "Kadıköy",
                        Price = 2_000_000,
                        RoomCount = 2,
                        Area = 100,
                        ImageUrl = "https://example.com/image1.jpg"
                    },
                    new Property
                    {
                        Id = 2,
                        Title = "Test Daire 2",
                        City = "Ankara",
                        District = "Çankaya",
                        Price = 3_000_000,
                        RoomCount = 3,
                        Area = 120,
                        ImageUrl = "https://example.com/image2.jpg"
                    }
                );
            }

            if (!context.Listings.Any())
            {
                context.Listings.AddRange(
                    new Listing
                    {
                        Id = 1,
                        Title = "Test İlan 1",
                        Description = "Test açıklama 1",
                        City = "İzmir",
                        Price = 1_500_000,
                        CreatedDate = DateTime.UtcNow
                    },
                    new Listing
                    {
                        Id = 2,
                        Title = "Test İlan 2",
                        Description = "Test açıklama 2",
                        City = "Bursa",
                        Price = 2_500_000,
                        CreatedDate = DateTime.UtcNow
                    }
                );
            }

            context.SaveChanges();
            return context;
        }
    }
}
