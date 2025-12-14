using Xunit;
using FluentAssertions;
using RealEstate.Api.Models;

namespace RealEstate.Api.Tests.Models
{
    public class PropertyTests
    {
        [Fact]
        public void Property_CanBeCreated_WithValidData()
        {
            // Arrange & Act
            var property = new Property
            {
                Id = 1,
                Title = "Test Daire",
                City = "İstanbul",
                District = "Kadıköy",
                Price = 5000000,
                RoomCount = 2,
                Area = 100,
                ImageUrl = "test.jpg"
            };

            // Assert
            property.Id.Should().Be(1);
            property.Title.Should().Be("Test Daire");
            property.City.Should().Be("İstanbul");
            property.District.Should().Be("Kadıköy");
            property.Price.Should().Be(5000000);
            property.RoomCount.Should().Be(2);
            property.Area.Should().Be(100);
            property.ImageUrl.Should().Be("test.jpg");
        }

        [Fact]
        public void Property_AllFieldsAreNotNull_WhenCreated()
        {
            // Arrange & Act
            var property = new Property
            {
                Id = 1,
                Title = "Daire",
                City = "Ankara",
                District = "Çankaya",
                Price = 3000000,
                RoomCount = 3,
                Area = 120,
                ImageUrl = "image.jpg"
            };

            // Assert
            property.Title.Should().NotBeNullOrEmpty();
            property.City.Should().NotBeNullOrEmpty();
            property.District.Should().NotBeNullOrEmpty();
            property.ImageUrl.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Property_NumericFieldsArePositive()
        {
            // Arrange & Act
            var property = new Property
            {
                Id = 5,
                Title = "Villa",
                City = "İzmir",
                District = "Bornova",
                Price = 8000000,
                RoomCount = 4,
                Area = 250,
                ImageUrl = "villa.jpg"
            };

            // Assert
            property.Id.Should().BePositive();
            property.Price.Should().BePositive();
            property.RoomCount.Should().BePositive();
            property.Area.Should().BePositive();
        }
    }
}
