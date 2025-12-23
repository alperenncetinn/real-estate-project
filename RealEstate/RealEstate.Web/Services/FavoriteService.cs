using System.Net.Http.Headers;
using System.Net.Http.Json;
using RealEstate.Web.Models;

namespace RealEstate.Web.Services
{
    public class FavoriteService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;
        private readonly ILogger<FavoriteService> _logger;

        public FavoriteService(HttpClient httpClient, AuthService authService, ILogger<FavoriteService> logger)
        {
            _httpClient = httpClient;
            _authService = authService;
            _logger = logger;
        }

        private void AttachBearer()
        {
            var token = _authService.GetToken();
            _httpClient.DefaultRequestHeaders.Authorization = null;

            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<bool> AddToFavoritesAsync(int listingId)
        {
            try
            {
                AttachBearer();
                var resp = await _httpClient.PostAsync($"api/favorites/{listingId}", null);
                return resp.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddToFavoritesAsync error");
                return false;
            }
        }

        public async Task<bool> RemoveFromFavoritesAsync(int listingId)
        {
            try
            {
                AttachBearer();
                var resp = await _httpClient.DeleteAsync($"api/favorites/{listingId}");
                return resp.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RemoveFromFavoritesAsync error");
                return false;
            }
        }

        public async Task<List<ListingViewModel>> GetMyFavoriteListingsAsync()
        {
            try
            {
                AttachBearer();

                // Önce status code'u görmek için GetAsync yapıyoruz
                var resp = await _httpClient.GetAsync("api/favorites");
                _logger.LogInformation("GetMyFavoriteListingsAsync -> Status: {Status}", resp.StatusCode);

                var body = await resp.Content.ReadAsStringAsync();
                if (!resp.IsSuccessStatusCode)
                {
                    if (!string.IsNullOrWhiteSpace(body))
                        _logger.LogInformation("GetMyFavoriteListingsAsync -> Body: {Body}", body);

                    return new List<ListingViewModel>();
                }

                // Başarılıysa JSON parse et
                var favorites = await resp.Content.ReadFromJsonAsync<List<FavoriteDto>>();
                if (favorites == null) return new List<ListingViewModel>();

                return favorites
                    .Where(x => x.Listing != null)
                    .Select(x => x.Listing!)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetMyFavoriteListingsAsync error");
                return new List<ListingViewModel>();
            }

        }

        // API'nin döndürdüğü Favorite shape
        public class FavoriteDto
        {
            public int Id { get; set; }
            public int UserId { get; set; }
            public int ListingId { get; set; }
            public DateTime CreatedAt { get; set; }
            public ListingViewModel? Listing { get; set; }
        }
    }
}
