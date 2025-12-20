using Microsoft.AspNetCore.Mvc;
using RealEstate.Web.Services;

namespace RealEstate.Web.Controllers
{
    public class NotificationController : Controller
    {
        private readonly NotificationService _notificationService;
        private readonly AuthService _authService;

        public NotificationController(NotificationService notificationService, AuthService authService)
        {
            _notificationService = notificationService;
            _authService = authService;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            if (!_authService.IsAuthenticated())
            {
                return Json(new List<object>());
            }

            var notifications = await _notificationService.GetNotificationsAsync();
            return Json(notifications);
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            if (!_authService.IsAuthenticated())
            {
                return Json(new { count = 0 });
            }

            var count = await _notificationService.GetUnreadCountAsync();
            return Json(new { count });
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            if (!_authService.IsAuthenticated())
            {
                return Unauthorized();
            }

            var result = await _notificationService.MarkAsReadAsync(id);
            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            if (!_authService.IsAuthenticated())
            {
                return Unauthorized();
            }

            var result = await _notificationService.MarkAllAsReadAsync();
            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!_authService.IsAuthenticated())
            {
                return Unauthorized();
            }

            var result = await _notificationService.DeleteAsync(id);
            return Json(new { success = result });
        }
    }
}
