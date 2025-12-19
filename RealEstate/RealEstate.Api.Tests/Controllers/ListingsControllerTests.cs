using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Moq;
using RealEstate.Api.Controllers;
using RealEstate.Api.Data;
using RealEstate.Api.Dtos;
using RealEstate.Api.Entities;
using RealEstate.Api.Tests.Helpers;
using System.Threading.Tasks;

namespace RealEstate.Api.Tests.Controllers
{
    public class ListingsControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<IWebHostEnvironment> _mockEnvironment;
        private readonly ListingsController _controller;

        public ListingsControllerTests()
        {
            _context = TestDbContextFactory.CreateContextWithData();
            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _mockEnvironment.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());
            _controller = new ListingsController(_context, _mockEnvironment.Object);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region GetAll Tests

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithListings()
        {
            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var listings = okResult.Value.Should().BeAssignableTo<IEnumerable<Listing>>().Subject;
            listings.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAll_ReturnsEmptyList_WhenNoListings()
        {
            // Arrange - Boş context oluştur (seed data olmadan)
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var emptyContext = new ApplicationDbContext(options);
            var controller = new ListingsController(emptyContext, _mockEnvironment.Object);

            // Act
            var result = await controller.GetAll();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var listings = okResult.Value.Should().BeAssignableTo<IEnumerable<Listing>>().Subject;
            listings.Should().BeEmpty();

            emptyContext.Dispose();
        }

        #endregion

        #region GetById Tests

        [Fact]
        public async Task GetById_ReturnsOkResult_WhenListingExists()
        {
            // Act
            var result = await _controller.GetById(1);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var listing = okResult.Value.Should().BeOfType<Listing>().Subject;
            listing.Id.Should().Be(1);
            listing.Title.Should().Be("Test İlan 1");
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenListingDoesNotExist()
        {
            // Act
            var result = await _controller.GetById(999);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        #endregion

        #region Create Tests

        [Fact]
        public async Task Create_ReturnsOkResult_WhenDtoIsValid()
        {
            // Arrange
            var dto = new ListingDtos
            {
                Title = "Yeni Test İlan",
                City = "Antalya",
                Price = 5_000_000,
                Description = "Yeni ilan açıklaması"
            };

            // Act
            var result = await _controller.Create(dto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();

            // Veritabanında oluşturulmuş mu kontrol et
            var createdListing = await _context.Listings.FindAsync(3);
            createdListing.Should().NotBeNull();
            createdListing!.Title.Should().Be("Yeni Test İlan");
            createdListing.City.Should().Be("Antalya");
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenDtoIsNull()
        {
            // Act
            var result = await _controller.Create(null!);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Create_SavesListingToDatabase()
        {
            // Arrange
            var initialCount = _context.Listings.Count();
            var dto = new ListingDtos
            {
                Title = "Database Test İlan",
                City = "Konya",
                Price = 1_000_000
            };

            // Act
            await _controller.Create(dto);

            // Assert
            _context.Listings.Count().Should().Be(initialCount + 1);
        }

        [Fact]
        public async Task Create_SetsCreatedDate()
        {
            // Arrange
            var dto = new ListingDtos
            {
                Title = "Tarih Test İlan",
                City = "Samsun",
                Price = 800_000
            };

            // Act
            var beforeCreate = DateTime.UtcNow;
            await _controller.Create(dto);
            var afterCreate = DateTime.UtcNow;

            // Assert
            var createdListing = _context.Listings.OrderByDescending(l => l.Id).First();
            createdListing.CreatedDate.Should().BeOnOrAfter(beforeCreate);
            createdListing.CreatedDate.Should().BeOnOrBefore(afterCreate);
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task Update_ReturnsOkResult_WhenListingExists()
        {
            // Arrange
            var dto = new ListingDtos
            {
                Title = "Güncellenmiş İlan",
                City = "Trabzon",
                Price = 2_000_000,
                Description = "Güncellenmiş açıklama"
            };

            // Act
            var result = await _controller.Update(1, dto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_UpdatesListingInDatabase()
        {
            // Arrange
            var dto = new ListingDtos
            {
                Title = "Değiştirilmiş Başlık",
                City = "Erzurum",
                Price = 3_500_000,
                Description = "Değiştirilmiş açıklama"
            };

            // Act
            await _controller.Update(1, dto);

            // Assert
            var updatedListing = await _context.Listings.FindAsync(1);
            updatedListing!.Title.Should().Be("Değiştirilmiş Başlık");
            updatedListing.City.Should().Be("Erzurum");
            updatedListing.Price.Should().Be(3_500_000);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenListingDoesNotExist()
        {
            // Arrange
            var dto = new ListingDtos { Title = "Test" };

            // Act
            var result = await _controller.Update(999, dto);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_ReturnsOkResult_WhenListingExists()
        {
            // Act
            var result = await _controller.Delete(1);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_RemovesListingFromDatabase()
        {
            // Arrange
            var initialCount = _context.Listings.Count();

            // Act
            await _controller.Delete(1);

            // Assert
            _context.Listings.Count().Should().Be(initialCount - 1);
            var deletedListing = await _context.Listings.FindAsync(1);
            deletedListing.Should().BeNull();
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenListingDoesNotExist()
        {
            // Act
            var result = await _controller.Delete(999);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        #endregion
    }
}