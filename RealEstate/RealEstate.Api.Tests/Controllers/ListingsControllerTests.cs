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
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

namespace RealEstate.Api.Tests.Controllers
{
    public class ListingsControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<IWebHostEnvironment> _mockEnvironment;
        private readonly ListingsController _controller;

        public ListingsControllerTests()
        {
            // Helper sınıfından dolu bir test veritabanı alıyoruz
            _context = TestDbContextFactory.CreateContextWithData();
            
            // Dosya işlemleri için sahte bir ortam (Mock)
            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _mockEnvironment.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());
            
            _controller = new ListingsController(_context, _mockEnvironment.Object);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region GetAll Tests (Listeleme & Filtreleme)

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithAllListings_WhenNoFilter()
        {
            // Act
            var result = await _controller.GetAll(null); // Filtre yok

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var listings = okResult.Value.Should().BeAssignableTo<IEnumerable<Listing>>().Subject;
            listings.Should().HaveCount(2); // Helper içinde 2 veri vardı
        }

        [Fact]
        public async Task GetAll_ReturnsFilteredListings_WhenTypeIsProvided()
        {
            // Act - Sadece "Kiralık" olanları iste
            var result = await _controller.GetAll("Kiralık");

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var listings = okResult.Value.Should().BeAssignableTo<IEnumerable<Listing>>().Subject;
            
            listings.Should().HaveCount(1); // Sadece 1 tane kiralık vardı
            listings.First().Type.Should().Be("Kiralık");
        }

        #endregion

        #region GetById Tests (Detay)

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

        #region Create Tests (Ekleme)

        [Fact]
        public async Task Create_ReturnsOkResult_WhenDtoIsValid()
        {
            // Arrange
            var dto = new ListingDtos
            {
                Title = "Yeni Test İlan",
                City = "Antalya",
                Price = 5_000_000,
                Type = "Satılık",
                Description = "Yeni ilan açıklaması"
            };

            // Act
            var result = await _controller.Create(dto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();

            // DB Kontrolü
            var createdListing = await _context.Listings.FirstOrDefaultAsync(l => l.Title == "Yeni Test İlan");
            createdListing.Should().NotBeNull();
            createdListing!.City.Should().Be("Antalya");
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenDtoIsNull()
        {
            // Act
            var result = await _controller.Create(null!);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        #endregion

        #region Update Tests (Güncelleme)

        [Fact]
        public async Task Update_ReturnsOkResult_WhenListingExists()
        {
            // Arrange
            var dto = new ListingDtos
            {
                Title = "Güncellenmiş İlan",
                City = "Trabzon",
                Price = 2_000_000,
                Type = "Satılık",
                Description = "Güncelleme testi"
            };

            // Act
            var result = await _controller.Update(1, dto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            
            // DB Kontrolü
            var updatedListing = await _context.Listings.FindAsync(1);
            updatedListing!.Title.Should().Be("Güncellenmiş İlan");
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenListingDoesNotExist()
        {
            // Act
            var result = await _controller.Update(999, new ListingDtos());

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        #endregion

        #region Delete Tests (Silme)

        [Fact]
        public async Task Delete_ReturnsOkResult_WhenListingExists()
        {
            // Act
            var result = await _controller.Delete(1);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            
            // DB Kontrolü
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