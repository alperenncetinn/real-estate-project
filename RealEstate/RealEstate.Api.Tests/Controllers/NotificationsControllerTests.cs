using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstate.Api.Controllers;
using RealEstate.Api.Data;
using RealEstate.Api.Entities;
using Xunit;

namespace RealEstate.Api.Tests.Controllers
{
    public class NotificationsControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationsController _controller;

        public NotificationsControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            _controller = new NotificationsController(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private void SetupUser(string userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task GetMyNotifications_ReturnsOnlyCurrentUser_UnreadFilter()
        {
            // Arrange
            SetupUser("1");
            _context.Notifications.AddRange(
                new Notification { UserId = 1, Title = "Mine-1", IsRead = false },
                new Notification { UserId = 1, Title = "Mine-2", IsRead = true },
                new Notification { UserId = 2, Title = "Other", IsRead = false }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetMyNotifications(unreadOnly: true);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var items = ok.Value.Should().BeAssignableTo<IEnumerable<object>>().Subject.ToList();
            items.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetUnreadCount_ReturnsOnlyOwnUnread()
        {
            SetupUser("5");
            _context.Notifications.AddRange(
                new Notification { UserId = 5, Title = "A", IsRead = false },
                new Notification { UserId = 5, Title = "B", IsRead = true },
                new Notification { UserId = 6, Title = "C", IsRead = false }
            );
            await _context.SaveChangesAsync();

            var result = await _controller.GetUnreadCount();

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = ok.Value!.GetType().GetProperty("count")!.GetValue(ok.Value);
            payload.Should().Be(1);
        }

        [Fact]
        public async Task MarkAsRead_ForbidsOtherUsersNotification()
        {
            SetupUser("10");
            var notif = new Notification { UserId = 11, Title = "Foreign", IsRead = false };
            _context.Notifications.Add(notif);
            await _context.SaveChangesAsync();

            var result = await _controller.MarkAsRead(notif.Id);
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task MarkAllAsRead_MarksAllOwnedNotifications()
        {
            SetupUser("15");
            _context.Notifications.AddRange(
                new Notification { UserId = 15, Title = "N1", IsRead = false },
                new Notification { UserId = 15, Title = "N2", IsRead = false },
                new Notification { UserId = 16, Title = "Other", IsRead = false }
            );
            await _context.SaveChangesAsync();

            var result = await _controller.MarkAllAsRead();
            result.Should().BeOfType<OkObjectResult>();

            var owned = await _context.Notifications.Where(n => n.UserId == 15).ToListAsync();
            owned.Should().OnlyContain(n => n.IsRead);
        }

        [Fact]
        public async Task Delete_RemovesOwnedNotification()
        {
            SetupUser("22");
            var notif = new Notification { UserId = 22, Title = "Delete", IsRead = false };
            _context.Notifications.Add(notif);
            await _context.SaveChangesAsync();

            var result = await _controller.Delete(notif.Id);

            result.Should().BeOfType<OkObjectResult>();
            (await _context.Notifications.FindAsync(notif.Id)).Should().BeNull();
        }
    }
}
