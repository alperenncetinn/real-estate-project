using Xunit;
using FluentAssertions;
using Moq;
using RealEstate.Api.Controllers;
using RealEstate.Api.Data;
using RealEstate.Api.Models;
using RealEstate.Api.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace RealEstate.Api.Tests.Controllers
{
    public class PropertiesControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<ILogger<PropertiesController>> _mockLogger;
        private readonly PropertiesController _controller;

        public PropertiesControllerTests()
        {
            _context = TestDbContextFactory.CreateContextWithData();
            _mockLogger = new Mock<ILogger<PropertiesController>>();
            _controller = new PropertiesController(_mockLogger.Object, _context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region GetAll Tests

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            // Act
            var result = await _controller.GetAll();

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_ReturnsPropertyList()
        {
            // Act
            var result = await _controller.GetAll();
            var okResult = result.Result as OkObjectResult;
            var properties = okResult!.Value as List<Property>;

            // Assert
            properties.Should().NotBeNull();
            properties.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetAll_ReturnsTwoProperties()
        {
            // Act
            var result = await _controller.GetAll();
            var okResult = result.Result as OkObjectResult;
            var properties = okResult!.Value as List<Property>;

            // Assert
            properties.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAll_EachPropertyHasValidData()
        {
            // Act
            var result = await _controller.GetAll();
            var okResult = result.Result as OkObjectResult;
            var properties = okResult!.Value as List<Property>;

            // Assert
            properties.Should().AllSatisfy(p =>
            {
                p.Id.Should().BeGreaterThan(0);
                p.Title.Should().NotBeNullOrEmpty();
                p.City.Should().NotBeNullOrEmpty();
                p.District.Should().NotBeNullOrEmpty();
                p.Price.Should().BeGreaterThan(0);
                p.RoomCount.Should().BeGreaterThan(0);
                p.Area.Should().BeGreaterThan(0);
                p.ImageUrl.Should().NotBeNullOrEmpty();
            });
        }

        [Fact]
        public async Task GetAll_ReturnsEmptyList_WhenNoProperties()
        {
            // Arrange - Boş context oluştur (seed data olmadan)
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var emptyContext = new ApplicationDbContext(options);
            var controller = new PropertiesController(_mockLogger.Object, emptyContext);

            // Act
            var result = await controller.GetAll();
            var okResult = result.Result as OkObjectResult;
            var properties = okResult!.Value as List<Property>;

            // Assert
            properties.Should().BeEmpty();

            emptyContext.Dispose();
        }

        #endregion

        #region GetById Tests

        [Fact]
        public async Task GetById_ReturnsOkResult_WhenPropertyExists()
        {
            // Act
            var result = await _controller.GetById(1);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsCorrectProperty()
        {
            // Arrange - Create a fresh context with our own test data
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_GetById_{Guid.NewGuid()}")
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Properties.Add(new Property
            {
                Id = 100,
                Title = "Test Daire 1",
                District = "Kadıköy",
                City = "İstanbul",
                Price = 1000000,
                Area = 100,
                RoomCount = 3,
                ImageUrl = "/images/test.jpg"
            });
            await context.SaveChangesAsync();

            var loggerMock = new Mock<ILogger<PropertiesController>>();
            var controller = new PropertiesController(loggerMock.Object, context);

            // Act
            var result = await controller.GetById(100);
            var okResult = result.Result as OkObjectResult;
            var property = okResult!.Value as Property;

            // Assert
            property.Should().NotBeNull();
            property!.Id.Should().Be(100);
            property.Title.Should().Be("Test Daire 1");
            property.City.Should().Be("İstanbul");
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenPropertyDoesNotExist()
        {
            // Act
            var result = await _controller.GetById(999);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        #endregion

        #region Create Tests

        [Fact]
        public async Task Create_ReturnsCreatedResult()
        {
            // Arrange
            var dto = new PropertyCreateDto
            {
                Title = "Yeni Test Daire",
                City = "Antalya",
                District = "Konyaaltı",
                Price = 5_000_000,
                RoomCount = "3+1",
                SquareMeters = 150
            };

            // Act
            var result = await _controller.Create(dto);

            // Assert
            result.Result.Should().BeOfType<CreatedAtActionResult>();
        }

        [Fact]
        public async Task Create_SavesPropertyToDatabase()
        {
            // Arrange
            var initialCount = _context.Properties.Count();
            var dto = new PropertyCreateDto
            {
                Title = "Database Test Property",
                City = "Konya",
                District = "Selçuklu",
                Price = 2_000_000,
                RoomCount = "2+1",
                SquareMeters = 100
            };

            // Act
            await _controller.Create(dto);

            // Assert
            _context.Properties.Count().Should().Be(initialCount + 1);
        }

        [Fact]
        public async Task Create_ParsesRoomCountCorrectly()
        {
            // Arrange
            var dto = new PropertyCreateDto
            {
                Title = "RoomCount Test",
                City = "Test",
                District = "Test",
                RoomCount = "4+1",
                SquareMeters = 200
            };

            // Act
            var result = await _controller.Create(dto);
            var createdResult = result.Result as CreatedAtActionResult;
            var property = createdResult!.Value as Property;

            // Assert
            property!.RoomCount.Should().Be(4);
        }

        [Fact]
        public async Task Create_SetsDefaultValues_WhenNotProvided()
        {
            // Arrange
            var dto = new PropertyCreateDto
            {
                Price = 1_000_000,
                SquareMeters = 80
            };

            // Act
            var result = await _controller.Create(dto);
            var createdResult = result.Result as CreatedAtActionResult;
            var property = createdResult!.Value as Property;

            // Assert
            property!.Title.Should().Be("Başlıksız İlan");
            property.City.Should().Be("Belirtilmedi");
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task Update_ReturnsOkResult_WhenPropertyExists()
        {
            // Arrange
            var dto = new PropertyCreateDto
            {
                Title = "Güncellenmiş Daire",
                City = "Mersin",
                District = "Yenişehir",
                Price = 4_000_000,
                RoomCount = "3+1",
                SquareMeters = 130
            };

            // Act
            var result = await _controller.Update(1, dto);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_UpdatesPropertyInDatabase()
        {
            // Arrange
            var dto = new PropertyCreateDto
            {
                Title = "Değiştirilmiş Başlık",
                City = "Diyarbakır",
                District = "Kayapınar",
                Price = 6_000_000,
                RoomCount = "5+1",
                SquareMeters = 200
            };

            // Act
            await _controller.Update(1, dto);

            // Assert
            var updatedProperty = await _context.Properties.FindAsync(1);
            updatedProperty!.Title.Should().Be("Değiştirilmiş Başlık");
            updatedProperty.City.Should().Be("Diyarbakır");
            updatedProperty.Price.Should().Be(6_000_000);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenPropertyDoesNotExist()
        {
            // Arrange
            var dto = new PropertyCreateDto { Title = "Test" };

            // Act
            var result = await _controller.Update(999, dto);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_ReturnsOkResult_WhenPropertyExists()
        {
            // Act
            var result = await _controller.Delete(1);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_RemovesPropertyFromDatabase()
        {
            // Arrange
            var initialCount = _context.Properties.Count();

            // Act
            await _controller.Delete(1);

            // Assert
            _context.Properties.Count().Should().Be(initialCount - 1);
            var deletedProperty = await _context.Properties.FindAsync(1);
            deletedProperty.Should().BeNull();
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenPropertyDoesNotExist()
        {
            // Act
            var result = await _controller.Delete(999);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        #endregion

        #region Logger Tests

        [Fact]
        public async Task GetAll_LogsInformation()
        {
            // Act
            await _controller.GetAll();

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.AtLeastOnce);
        }

        #endregion
    }
}
