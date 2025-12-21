using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using RealEstate.Api.Controllers;
using RealEstate.Api.Data;
using RealEstate.Api.Dtos;
using RealEstate.Api.Entities;
using RealEstate.Api.Services;
using RealEstate.Api.Tests.Helpers;
using Xunit;

namespace RealEstate.Api.Tests.Controllers
{
    public class AuthControllerRegisterTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<IJwtService> _jwtService;
        private readonly IConfiguration _configuration;
        private readonly AuthController _controller;

        public AuthControllerRegisterTests()
        {
            _context = TestDbContextFactory.CreateContext();

            _jwtService = new Mock<IJwtService>();
            _jwtService.Setup(s => s.GenerateToken(It.IsAny<User>())).Returns("TOKEN");

            var settings = new Dictionary<string, string?>
            {
                { "JwtSettings:ExpirationMinutes", "120" }
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();

            _controller = new AuthController(_context, _jwtService.Object, _configuration);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private void SetupUser(string userId, string role = "Admin")
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        [Fact]
        public async Task Register_Success_CreatesUserAndReturnsToken()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                Email = "new@mail.com",
                Password = "Strong123",
                FirstName = "New",
                LastName = "User"
            };

            // Act
            var result = await _controller.Register(request);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = ok.Value.Should().BeOfType<LoginResponseDto>().Subject;
            response.Email.Should().Be(request.Email);
            response.Role.Should().Be("User");
            _jwtService.Verify(s => s.GenerateToken(It.Is<User>(u => u.Email == request.Email)), Times.Once);

            var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            dbUser.Should().NotBeNull();
            dbUser!.PasswordHash.Should().NotBe(request.Password);
        }

        [Fact]
        public async Task Register_Fails_WhenEmailAlreadyUsed()
        {
            // Arrange
            _context.Users.Add(new User
            {
                Email = "duplicate@mail.com",
                PasswordHash = HashPassword("secret"),
                FirstName = "Old",
                LastName = "User",
                Role = "User",
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            });
            await _context.SaveChangesAsync();

            var request = new RegisterRequestDto
            {
                Email = "duplicate@mail.com",
                Password = "Strong123",
                FirstName = "New",
                LastName = "User"
            };

            // Act
            var result = await _controller.Register(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Register_Fails_WhenBodyIsNull()
        {
            var result = await _controller.Register(null!);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetCurrentUser_ReturnsUser_WhenExists()
        {
            // Arrange
            var user = new User
            {
                Id = 5,
                Email = "me@test.com",
                PasswordHash = HashPassword("pwd"),
                FirstName = "Me",
                LastName = "User",
                Role = "User",
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            SetupUser(user.Id.ToString(), "User");

            // Act
            var result = await _controller.GetCurrentUser();

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var dto = ok.Value.Should().BeOfType<UserDto>().Subject;
            dto.Id.Should().Be(user.Id);
            dto.Email.Should().Be(user.Email);
        }

        [Fact]
        public async Task GetCurrentUser_ReturnsNotFound_WhenUserMissing()
        {
            SetupUser("9", "User");
            var result = await _controller.GetCurrentUser();
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task UpdateUserRole_AllowsAdminAndPersists()
        {
            // Arrange
            var target = new User
            {
                Id = 20,
                Email = "user20@mail.com",
                PasswordHash = HashPassword("pwd"),
                FirstName = "U",
                LastName = "20",
                Role = "User",
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };
            _context.Users.Add(target);
            await _context.SaveChangesAsync();

            SetupUser("1", "Admin");

            var dto = new UpdateUserRoleDto { Role = "Admin" };

            // Act
            var result = await _controller.UpdateUserRole(target.Id, dto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var reloaded = await _context.Users.FindAsync(target.Id);
            reloaded!.Role.Should().Be("Admin");
        }

        [Fact]
        public async Task UpdateUserRole_InvalidRole_ReturnsBadRequest()
        {
            SetupUser("1", "Admin");
            var dto = new UpdateUserRoleDto { Role = "SuperUser" };

            var result = await _controller.UpdateUserRole(999, dto);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task DeactivateUser_StopsSelfDeactivation()
        {
            // Arrange
            var admin = new User
            {
                Id = 50,
                Email = "admin@mail.com",
                PasswordHash = HashPassword("pwd"),
                FirstName = "Admin",
                LastName = "Self",
                Role = "Admin",
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };
            _context.Users.Add(admin);
            await _context.SaveChangesAsync();

            SetupUser(admin.Id.ToString(), "Admin");

            // Act
            var result = await _controller.DeactivateUser(admin.Id);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task DeactivateUser_MarksInactive_ForAnotherUser()
        {
            var admin = new User
            {
                Id = 51,
                Email = "admin2@mail.com",
                PasswordHash = HashPassword("pwd"),
                FirstName = "Admin",
                LastName = "Two",
                Role = "Admin",
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };
            var target = new User
            {
                Id = 52,
                Email = "user52@mail.com",
                PasswordHash = HashPassword("pwd"),
                FirstName = "User",
                LastName = "Target",
                Role = "User",
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };
            _context.Users.AddRange(admin, target);
            await _context.SaveChangesAsync();

            SetupUser(admin.Id.ToString(), "Admin");

            var result = await _controller.DeactivateUser(target.Id);

            result.Should().BeOfType<OkObjectResult>();
            var refreshed = await _context.Users.FindAsync(target.Id);
            refreshed!.IsActive.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteUser_PreventsSelfDelete()
        {
            var admin = new User
            {
                Id = 60,
                Email = "admin3@mail.com",
                PasswordHash = HashPassword("pwd"),
                FirstName = "Admin",
                LastName = "Three",
                Role = "Admin",
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };
            _context.Users.Add(admin);
            await _context.SaveChangesAsync();

            SetupUser(admin.Id.ToString(), "Admin");

            var result = await _controller.DeleteUser(admin.Id);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task DeleteUser_RemovesTargetUser()
        {
            var admin = new User
            {
                Id = 70,
                Email = "admin4@mail.com",
                PasswordHash = HashPassword("pwd"),
                FirstName = "Admin",
                LastName = "Four",
                Role = "Admin",
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };
            var target = new User
            {
                Id = 71,
                Email = "victim@mail.com",
                PasswordHash = HashPassword("pwd"),
                FirstName = "Victim",
                LastName = "User",
                Role = "User",
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };
            _context.Users.AddRange(admin, target);
            await _context.SaveChangesAsync();

            SetupUser(admin.Id.ToString(), "Admin");

            var result = await _controller.DeleteUser(target.Id);
            result.Should().BeOfType<OkObjectResult>();
            (await _context.Users.FindAsync(target.Id)).Should().BeNull();
        }

        [Fact]
        public async Task Authentication_Flow_RegisterThenGetCurrentUser_Works()
        {
            // Register
            var register = await _controller.Register(new RegisterRequestDto
            {
                Email = "flow@mail.com",
                Password = "Flow123",
                FirstName = "Flow",
                LastName = "User"
            });

            var registerOk = register.Should().BeOfType<OkObjectResult>().Subject;
            var loginResponse = registerOk.Value.Should().BeOfType<LoginResponseDto>().Subject;

            // Simulate authenticated call to /me
            SetupUser(loginResponse.Id.ToString(), loginResponse.Role);
            var me = await _controller.GetCurrentUser();

            var meOk = me.Should().BeOfType<OkObjectResult>().Subject;
            var userDto = meOk.Value.Should().BeOfType<UserDto>().Subject;
            userDto.Email.Should().Be("flow@mail.com");
        }
    }
}
