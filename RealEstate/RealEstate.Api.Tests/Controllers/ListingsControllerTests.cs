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
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace RealEstate.Api.Tests.Controllers
{
    public class ListingsControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<IWebHostEnvironment> _mockEnvironment;
        private readonly ListingsController _controller;

        public ListingsControllerTests()
        {
            // 1. Her test için benzersiz bir veritabanı oluştur
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            // 2. Mock Ortam
            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _mockEnvironment.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());
            _mockEnvironment.Setup(e => e.ContentRootPath).Returns(Path.GetTempPath());

            _controller = new ListingsController(_context, _mockEnvironment.Object);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        // Helper: Kullanıcıyı Login olmuş gibi göster
        private void SetupUser(string userId, string role = "User")
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var user = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        // Helper: Veritabanına Test İçin Kullanıcı Ekle (Foreign Key hatasını önlemek için)
        private async Task EnsureUserExists(int userId)
        {
            if (!await _context.Users.AnyAsync(u => u.Id == userId))
            {
                _context.Users.Add(new User 
                { 
                    Id = userId, 
                    FirstName = "Test", 
                    LastName = "User", 
                    Email = "test@user.com", 
                    PasswordHash = "hash" // Zorunlu alanları doldur
                });
                await _context.SaveChangesAsync();
            }
        }

        #region GetAll Tests

        [Fact]
        public async Task GetAll_ReturnsOk_WithListings()
        {
            // Arrange
            SetupUser("1");
            await EnsureUserExists(1); // Kullanıcıyı ekle

            // İlanları ekle
            _context.Listings.Add(new Listing { Title = "Ev 1", UserId = 1, IsActive = true, Type = "Satılık", Description = "Test", City="Ist", Price=100 });
            _context.Listings.Add(new Listing { Title = "Ev 2", UserId = 1, IsActive = true, Type = "Kiralık", Description = "Test", City="Ank", Price=200 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetAll(null);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var items = okResult.Value.Should().BeAssignableTo<IEnumerable<object>>().Subject;
            items.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAll_FiltersByType()
        {
            // Arrange
            SetupUser("1");
            await EnsureUserExists(1);

            _context.Listings.Add(new Listing { Title = "Satılık Ev", Type = "Satılık", UserId = 1, IsActive = true, Description = "Desc", City="X", Price=10 });
            _context.Listings.Add(new Listing { Title = "Kiralık Ev", Type = "Kiralık", UserId = 1, IsActive = true, Description = "Desc", City="Y", Price=20 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetAll("Kiralık");

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var items = okResult.Value.Should().BeAssignableTo<IEnumerable<object>>().Subject;
            items.Should().HaveCount(1); // Sadece 1 tane kiralık gelmeli
        }

        #endregion

        #region GetById Tests

        [Fact]
        public async Task GetById_ReturnsOk_WhenExists()
        {
            // Arrange
            SetupUser("1");
            await EnsureUserExists(1);

            // İlanı ekle ve ID'yi veritabanının vermesine izin ver
            var listing = new Listing { Title = "Detay İlan", UserId = 1, IsActive = true, Description = "D", Type = "S", City = "C", Price=500, SquareMeters=100 };
            _context.Listings.Add(listing);
            await _context.SaveChangesAsync(); 
            
            // DİKKAT: Elle ID (10) vermek yerine, DB'nin atadığı ID'yi (listing.Id) alıyoruz.
            var generatedId = listing.Id;

            // Act
            var result = await _controller.GetById(generatedId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenNotExists()
        {
            SetupUser("1");
            var result = await _controller.GetById(9999); // Olmayan ID
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        #endregion

        #region Create Tests

        [Fact]
        public async Task Create_SavesRoomCountAndSquareMeters()
        {
            // Arrange
            SetupUser("5"); // ID: 5 olan kullanıcı ekliyor
            await EnsureUserExists(5); // DB'ye 5 nolu kullanıcıyı ekle

            var dto = new ListingDtos
            {
                Title = "Full İlan",
                City = "İzmir",
                Price = 100,
                Type = "Satılık",
                Description = "Açıklama",
                RoomCount = "4+1",
                SquareMeters = 180
            };

            // Act
            var result = await _controller.Create(dto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();

            // DB'den kontrol et
            var dbListing = await _context.Listings.FirstOrDefaultAsync(l => l.Title == "Full İlan");
            dbListing.Should().NotBeNull();
            dbListing!.RoomCount.Should().Be("4+1");
            dbListing.SquareMeters.Should().Be(180);
            dbListing.UserId.Should().Be(5);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_IfNull()
        {
            SetupUser("1");
            var result = await _controller.Create(null!);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task Update_UpdatesFieldsCorrectly()
        {
            // Arrange
            SetupUser("2"); 
            await EnsureUserExists(2);
            
            var existing = new Listing { Title = "Eski", UserId = 2, IsActive = true, Description = "D", Type = "S", City = "C", SquareMeters = 50, RoomCount = "1+1", Price=100 };
            _context.Listings.Add(existing);
            await _context.SaveChangesAsync();
            
            // Context takibini temizle (fresh data)
            _context.Entry(existing).State = EntityState.Detached;
            var idToUpdate = existing.Id;

            var dto = new ListingDtos
            {
                Title = "Yeni Başlık",
                City = "Yeni Şehir",
                Price = 500,
                Type = "Kiralık",
                Description = "Yeni Açıklama",
                RoomCount = "3+1",
                SquareMeters = 120
            };

            // Act
            var result = await _controller.Update(idToUpdate, dto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();

            var updated = await _context.Listings.FindAsync(idToUpdate);
            updated!.Title.Should().Be("Yeni Başlık");
            updated.RoomCount.Should().Be("3+1");
            updated.SquareMeters.Should().Be(120);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_RemovesListing()
        {
            // Arrange
            SetupUser("3");
            await EnsureUserExists(3);

            var listing = new Listing { Title = "Sil Beni", UserId = 3, IsActive = true, Description = "D", Type = "S", City = "C", Price=50 };
            _context.Listings.Add(listing);
            await _context.SaveChangesAsync();
            
            var idToDelete = listing.Id;
            _context.Entry(listing).State = EntityState.Detached;

            // Act
            var result = await _controller.Delete(idToDelete);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var check = await _context.Listings.FindAsync(idToDelete);
            check.Should().BeNull();
        }

        #endregion
    }
}