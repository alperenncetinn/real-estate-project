using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Moq;
using RealEstate.Api.Controllers;
using RealEstate.Api.Data;
using RealEstate.Api.Dtos;
using RealEstate.Api.Entities;
using RealEstate.Api.Services;
using RealEstate.Api.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RealEstate.Api.Tests.Controllers
{
    public class AuthControllerLoginTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<IJwtService> _mockJwtService;
        private readonly IConfiguration _configuration;
        private readonly AuthController _controller;

        public AuthControllerLoginTests()
        {
            // 1) Test DB (Listings testleriyle AYNI)
            _context = TestDbContextFactory.CreateContextWithData();

            // 2) JWT mock (sahte token üret)
            _mockJwtService = new Mock<IJwtService>();
            _mockJwtService
                .Setup(s => s.GenerateToken(It.IsAny<User>()))
                .Returns("TEST_TOKEN");

            // 3) Config (ExpirationMinutes lazım)
            var configData = new Dictionary<string, string?>
            {
                { "JwtSettings:ExpirationMinutes", "60" }
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            // 4) Controller
            _controller = new AuthController(
                _context,
                _mockJwtService.Object,
                _configuration
            );
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        // AuthController ile BİREBİR aynı hash
        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        [Fact]
        public async Task Login_Success_ReturnsOk()
        {
            // Arrange
            var user = new User
            {
                Email = "test@mail.com",
                PasswordHash = HashPassword("123456"),
                FirstName = "Test",
                LastName = "User",
                Role = "User",
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var request = new LoginRequestDto
            {
                Email = "test@mail.com",
                Password = "123456"
            };

            // Act
            var result = await _controller.Login(request);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = ok.Value.Should().BeOfType<LoginResponseDto>().Subject;

            response.Email.Should().Be(user.Email);
            response.Token.Should().Be("TEST_TOKEN");
        }

        [Fact]
        public async Task Login_Fails_WhenPasswordIsWrong()
        {
            var user = new User
            {
                Email = "test@mail.com",
                PasswordHash = HashPassword("123456"),
                FirstName = "Test",
                LastName = "User",
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var request = new LoginRequestDto
            {
                Email = "test@mail.com",
                Password = "WRONG"
            };

            var result = await _controller.Login(request);

            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Theory]
        [InlineData("", "123456")]
        [InlineData("test@mail.com", "")]
        [InlineData("", "")]
        public async Task Login_Fails_WhenFieldsAreEmpty(string email, string password)
        {
            var request = new LoginRequestDto
            {
                Email = email,
                Password = password
            };

            var result = await _controller.Login(request);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Login_Fails_WhenRequestBodyIsNull()
        {
            var result = await _controller.Login(null!);

            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
