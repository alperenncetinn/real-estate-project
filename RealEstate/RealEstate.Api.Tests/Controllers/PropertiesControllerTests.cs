using Xunit;
using FluentAssertions;
using Moq;
using RealEstate.Api.Controllers;
using RealEstate.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace RealEstate.Api.Tests.Controllers
{
    public class PropertiesControllerTests
    {
        private readonly Mock<ILogger<PropertiesController>> _mockLogger;
        private readonly PropertiesController _controller;

        public PropertiesControllerTests()
        {
            _mockLogger = new Mock<ILogger<PropertiesController>>();
            _controller = new PropertiesController(_mockLogger.Object);
        }

        [Fact]
        public void GetAll_ReturnsOkResult()
        {
            // Arrange - Hazırlık yapıldı (constructor'da)

            // Act - Aksiyonu gerçekleştir
            var result = _controller.GetAll();

            // Assert - Kontrol et
            result.Should().NotBeNull();
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);
        }

        [Fact]
        public void GetAll_ReturnsPropertyList()
        {
            // Arrange
            // (constructor'da hazırlandı)

            // Act
            var result = _controller.GetAll();
            var okResult = result.Result as OkObjectResult;
            var properties = okResult!.Value as List<Property>;

            // Assert
            properties.Should().NotBeNull();
            properties.Should().NotBeEmpty();
        }

        [Fact]
        public void GetAll_ReturnsAtLeastTwoProperties()
        {
            // Arrange
            // (constructor'da hazırlandı)

            // Act
            var result = _controller.GetAll();
            var okResult = result.Result as OkObjectResult;
            var properties = okResult!.Value as List<Property>;

            // Assert
            properties.Should().HaveCountGreaterThanOrEqualTo(2);
        }

        [Fact]
        public void GetAll_EachPropertyHasValidData()
        {
            // Arrange
            // (constructor'da hazırlandı)

            // Act
            var result = _controller.GetAll();
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
        public void GetAll_LogsInformation()
        {
            // Arrange
            // (constructor'da hazırlandı)

            // Act
            var result = _controller.GetAll();

            // Assert - Logger'ın çağrıldığını kontrol et
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
