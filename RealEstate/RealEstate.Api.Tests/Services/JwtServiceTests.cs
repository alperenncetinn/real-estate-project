using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using RealEstate.Api.Entities;
using RealEstate.Api.Services;
using Xunit;

namespace RealEstate.Api.Tests.Services
{
    public class JwtServiceTests
    {
        [Fact]
        public void GenerateToken_ContainsExpectedClaimsAndMetadata()
        {
            // Arrange
            var configData = new Dictionary<string, string?>
            {
                { "JwtSettings:SecretKey", "supersecretkey_supersecretkey_12345" },
                { "JwtSettings:Issuer", "realestate-api" },
                { "JwtSettings:Audience", "realestate-web" },
                { "JwtSettings:ExpirationMinutes", "30" }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            var service = new JwtService(configuration);
            var user = new User
            {
                Id = 10,
                Email = "user@mail.com",
                PasswordHash = "hash",
                FirstName = "Test",
                LastName = "User",
                Role = "Admin"
            };

            // Act
            var token = service.GenerateToken(user);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            jwt.Issuer.Should().Be("realestate-api");
            jwt.Audiences.Should().ContainSingle(a => a == "realestate-web");

            var claims = jwt.Claims.ToDictionary(c => c.Type, c => c.Value);
            claims[ClaimTypes.NameIdentifier].Should().Be("10");
            claims[ClaimTypes.Email].Should().Be("user@mail.com");
            claims[ClaimTypes.Role].Should().Be("Admin");
            claims["firstName"].Should().Be("Test");
            claims["lastName"].Should().Be("User");

            var expectedExpiry = DateTime.UtcNow.AddMinutes(30);
            jwt.ValidTo.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(5));
        }
    }
}
