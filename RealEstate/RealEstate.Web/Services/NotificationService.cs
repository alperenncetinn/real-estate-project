using System.Net.Http.Json;
using System.Text.Json;

namespace RealEstate.Web.Services
{
    public class NotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<NotificationService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private void SetAuthHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Request.Cookies["auth_token"];
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<List<NotificationDto>> GetNotificationsAsync(bool unreadOnly = false)
        {
            try
            {
                SetAuthHeader();
                var url = unreadOnly ? "api/notifications?unreadOnly=true" : "api/notifications";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<NotificationDto>>() ?? new List<NotificationDto>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching notifications");
            }
            return new List<NotificationDto>();
        }

        public async Task<int> GetUnreadCountAsync()
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.GetAsync("api/notifications/unread-count");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<UnreadCountDto>();
                    return result?.Count ?? 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching unread count");
            }
            return 0;
        }

        public async Task<bool> MarkAsReadAsync(int notificationId)
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.PutAsync($"api/notifications/{notificationId}/read", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
            }
            return false;
        }

        public async Task<bool> MarkAllAsReadAsync()
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.PutAsync("api/notifications/read-all", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
            }
            return false;
        }

        public async Task<bool> DeleteAsync(int notificationId)
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.DeleteAsync($"api/notifications/{notificationId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification");
            }
            return false;
        }
    }

    public class NotificationDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "info";
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? ListingId { get; set; }
    }

    public class UnreadCountDto
    {
        public int Count { get; set; }
    }
}
