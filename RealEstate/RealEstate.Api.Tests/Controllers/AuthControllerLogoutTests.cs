using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using RealEstate.Api.Controllers;
using RealEstate.Api.Data;
using RealEstate.Api.Entities;
using RealEstate.Api.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace RealEstate.Api.Tests.Controllers
{
    public class AuthControllerLogoutTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<IJwtService> _mockJwtService;
        private readonly IConfiguration _configuration;
        private readonly AuthController _controller;

        public AuthControllerLogoutTests()
        {
            // 1) InMemory DB kur (helper yoksa en temiz yol bu)
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"RealEstateTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new ApplicationDbContext(options);

            // 2) JWT mock (logout kullanmıyor ama controller ctor istiyor)
            _mockJwtService = new Mock<IJwtService>();

            // 3) Configuration (logout kullanmıyor ama controller ctor istiyor)
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "JwtSettings:ExpirationMinutes", "60" }
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _controller = new AuthController(_context, _mockJwtService.Object, _configuration);

            // 4) [Authorize] için sahte (fake) kullanıcı ekle (unit testte middleware yok)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Email, "test@test.com"),
                new Claim(ClaimTypes.Role, "User")
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // DB’ye örnek bir user ekleyelim (stateless testinde sayı kontrolü için)
            _context.Users.Add(new User
            {
                Id = 1,
                Email = "test@test.com",
                PasswordHash = "dummyhash",
                FirstName = "Test",
                LastName = "User",
                Role = "User",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            });

            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public void Logout_Success_ReturnsOk()
        {
            // Act
            var result = _controller.Logout();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void Logout_Response_IsJson_AndMessageCorrect()
        {
            // Act
            var result = _controller.Logout();

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;

            // anonymous object => reflection ile "message" okuruz
            ok.Value.Should().NotBeNull();
            var messageProp = ok.Value!.GetType().GetProperty("message");
            messageProp.Should().NotBeNull("Logout response içinde 'message' alanı olmalı");

            var message = messageProp!.GetValue(ok.Value)?.ToString();
            message.Should().Be("Logout successful.");
        }

        [Fact]
        public void Logout_IsStateless_DoesNotModifyDatabase()
        {
            // Arrange
            var usersBefore = _context.Users.Count();

            // Act
            _controller.Logout();

            // Assert
            var usersAfter = _context.Users.Count();
            usersAfter.Should().Be(usersBefore);
        }
    }
}
