using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstate.Api.Data;
using System.Security.Claims;

namespace RealEstate.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NotificationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
                return userId;
            return null;
        }

        // Kullanıcının bildirimlerini getir
        [HttpGet]
        public async Task<IActionResult> GetMyNotifications([FromQuery] bool unreadOnly = false)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var query = _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .AsQueryable();

            if (unreadOnly)
            {
                query = query.Where(n => !n.IsRead);
            }

            var notifications = await query.Select(n => new
            {
                n.Id,
                n.Title,
                n.Message,
                n.Type,
                n.IsRead,
                n.CreatedAt,
                n.ListingId
            }).ToListAsync();

            return Ok(notifications);
        }

        // Okunmamış bildirim sayısı
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var count = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();

            return Ok(new { count });
        }

        // Bildirimi okundu olarak işaretle
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return NotFound();

            if (notification.UserId != userId)
            {
                return Forbid();
            }

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Bildirim okundu olarak isaretlendi." });
        }

        // Tüm bildirimleri okundu olarak işaretle
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = $"{notifications.Count} bildirim okundu olarak isaretlendi." });
        }

        // Bildirimi sil
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return NotFound();

            if (notification.UserId != userId)
            {
                return Forbid();
            }

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Bildirim silindi." });
        }
    }
}
