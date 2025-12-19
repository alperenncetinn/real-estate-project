using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RealEstate.Api.Data;
using RealEstate.Api.Entities;
using RealEstate.Api.Models;
using RealEstate.Api.Tests.Helpers;

namespace RealEstate.Api.Tests.Data
{
    public class ApplicationDbContextTests : IDisposable
    {
        private readonly ApplicationDbContext _context;

        public ApplicationDbContextTests()
        {
            _context = TestDbContextFactory.CreateContext();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region DbSet Tests

        [Fact]
        public void DbContext_HasListingsDbSet()
        {
            // Assert
            _context.Listings.Should().NotBeNull();
        }

        [Fact]
        public void DbContext_HasPropertiesDbSet()
        {
            // Assert
            _context.Properties.Should().NotBeNull();
        }

        #endregion

        #region Listing CRUD Tests

        [Fact]
        public async Task Listings_CanAddNewListing()
        {
            // Arrange
            var listing = new Listing
            {
                Title = "Test Listing",
                Description = "Test Description",
                City = "İstanbul",
                Price = 1_000_000,
                CreatedDate = DateTime.UtcNow
            };

            // Act
            _context.Listings.Add(listing);
            await _context.SaveChangesAsync();

            // Assert
            var savedListing = await _context.Listings.FindAsync(listing.Id);
            savedListing.Should().NotBeNull();
            savedListing!.Title.Should().Be("Test Listing");
        }

        [Fact]
        public async Task Listings_CanUpdateListing()
        {
            // Arrange
            var listing = new Listing
            {
                Title = "Original Title",
                City = "Ankara",
                Price = 500_000,
                CreatedDate = DateTime.UtcNow
            };
            _context.Listings.Add(listing);
            await _context.SaveChangesAsync();

            // Act
            listing.Title = "Updated Title";
            listing.Price = 600_000;
            await _context.SaveChangesAsync();

            // Assert
            var updatedListing = await _context.Listings.FindAsync(listing.Id);
            updatedListing!.Title.Should().Be("Updated Title");
            updatedListing.Price.Should().Be(600_000);
        }

        [Fact]
        public async Task Listings_CanDeleteListing()
        {
            // Arrange
            var listing = new Listing
            {
                Title = "To Be Deleted",
                City = "İzmir",
                Price = 300_000,
                CreatedDate = DateTime.UtcNow
            };
            _context.Listings.Add(listing);
            await _context.SaveChangesAsync();
            var listingId = listing.Id;

            // Act
            _context.Listings.Remove(listing);
            await _context.SaveChangesAsync();

            // Assert
            var deletedListing = await _context.Listings.FindAsync(listingId);
            deletedListing.Should().BeNull();
        }

        [Fact]
        public async Task Listings_CanQueryWithLinq()
        {
            // Arrange
            _context.Listings.AddRange(
                new Listing { Title = "Cheap", City = "Test", Price = 100_000, CreatedDate = DateTime.UtcNow },
                new Listing { Title = "Expensive", City = "Test", Price = 5_000_000, CreatedDate = DateTime.UtcNow },
                new Listing { Title = "Medium", City = "Test", Price = 1_000_000, CreatedDate = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();

            // Act
            var expensiveListings = await _context.Listings
                .Where(l => l.Price > 500_000)
                .OrderByDescending(l => l.Price)
                .ToListAsync();

            // Assert
            expensiveListings.Should().HaveCount(2);
            expensiveListings.First().Title.Should().Be("Expensive");
        }

        #endregion

        #region Property CRUD Tests

        [Fact]
        public async Task Properties_CanAddNewProperty()
        {
            // Arrange
            var property = new Property
            {
                Title = "Test Property",
                City = "Bursa",
                District = "Nilüfer",
                Price = 2_000_000,
                RoomCount = 3,
                Area = 120,
                ImageUrl = "https://example.com/test.jpg"
            };

            // Act
            _context.Properties.Add(property);
            await _context.SaveChangesAsync();

            // Assert
            var savedProperty = await _context.Properties.FindAsync(property.Id);
            savedProperty.Should().NotBeNull();
            savedProperty!.Title.Should().Be("Test Property");
            savedProperty.District.Should().Be("Nilüfer");
        }

        [Fact]
        public async Task Properties_CanUpdateProperty()
        {
            // Arrange
            var property = new Property
            {
                Title = "Original Property",
                City = "Kayseri",
                District = "Melikgazi",
                Price = 1_500_000,
                RoomCount = 2,
                Area = 90,
                ImageUrl = "https://example.com/original.jpg"
            };
            _context.Properties.Add(property);
            await _context.SaveChangesAsync();

            // Act
            property.Title = "Updated Property";
            property.Price = 1_800_000;
            property.RoomCount = 3;
            await _context.SaveChangesAsync();

            // Assert
            var updatedProperty = await _context.Properties.FindAsync(property.Id);
            updatedProperty!.Title.Should().Be("Updated Property");
            updatedProperty.Price.Should().Be(1_800_000);
            updatedProperty.RoomCount.Should().Be(3);
        }

        [Fact]
        public async Task Properties_CanDeleteProperty()
        {
            // Arrange
            var property = new Property
            {
                Title = "Property To Delete",
                City = "Eskişehir",
                District = "Tepebaşı",
                Price = 900_000,
                RoomCount = 2,
                Area = 80,
                ImageUrl = "https://example.com/delete.jpg"
            };
            _context.Properties.Add(property);
            await _context.SaveChangesAsync();
            var propertyId = property.Id;

            // Act
            _context.Properties.Remove(property);
            await _context.SaveChangesAsync();

            // Assert
            var deletedProperty = await _context.Properties.FindAsync(propertyId);
            deletedProperty.Should().BeNull();
        }

        [Fact]
        public async Task Properties_CanFilterByCity()
        {
            // Arrange
            _context.Properties.AddRange(
                new Property { Title = "P1", City = "İstanbul", District = "Kadıköy", Price = 1_000_000, RoomCount = 2, Area = 80, ImageUrl = "url1" },
                new Property { Title = "P2", City = "İstanbul", District = "Beşiktaş", Price = 2_000_000, RoomCount = 3, Area = 100, ImageUrl = "url2" },
                new Property { Title = "P3", City = "Ankara", District = "Çankaya", Price = 1_500_000, RoomCount = 2, Area = 90, ImageUrl = "url3" }
            );
            await _context.SaveChangesAsync();

            // Act
            var istanbulProperties = await _context.Properties
                .Where(p => p.City == "İstanbul")
                .ToListAsync();

            // Assert
            istanbulProperties.Should().HaveCount(2);
            istanbulProperties.Should().AllSatisfy(p => p.City.Should().Be("İstanbul"));
        }

        [Fact]
        public async Task Properties_CanFilterByPriceRange()
        {
            // Arrange
            _context.Properties.AddRange(
                new Property { Title = "Cheap", City = "Test", District = "Test", Price = 500_000, RoomCount = 1, Area = 50, ImageUrl = "url1" },
                new Property { Title = "Medium", City = "Test", District = "Test", Price = 1_500_000, RoomCount = 2, Area = 80, ImageUrl = "url2" },
                new Property { Title = "Expensive", City = "Test", District = "Test", Price = 5_000_000, RoomCount = 4, Area = 200, ImageUrl = "url3" }
            );
            await _context.SaveChangesAsync();

            // Act
            var midRangeProperties = await _context.Properties
                .Where(p => p.Price >= 1_000_000 && p.Price <= 2_000_000)
                .ToListAsync();

            // Assert
            midRangeProperties.Should().HaveCount(1);
            midRangeProperties.First().Title.Should().Be("Medium");
        }

        #endregion

        #region Entity Configuration Tests

        [Fact]
        public async Task Listing_IdIsAutoGenerated()
        {
            // Arrange
            var listing = new Listing
            {
                Title = "Auto ID Test",
                City = "Test",
                Price = 100_000,
                CreatedDate = DateTime.UtcNow
            };

            // Act
            _context.Listings.Add(listing);
            await _context.SaveChangesAsync();

            // Assert
            listing.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Property_IdIsAutoGenerated()
        {
            // Arrange
            var property = new Property
            {
                Title = "Auto ID Test",
                City = "Test",
                District = "Test",
                Price = 100_000,
                RoomCount = 1,
                Area = 50,
                ImageUrl = "url"
            };

            // Act
            _context.Properties.Add(property);
            await _context.SaveChangesAsync();

            // Assert
            property.Id.Should().BeGreaterThan(0);
        }

        #endregion
    }
}
