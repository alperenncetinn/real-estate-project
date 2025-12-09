using System.Net.Http.Json;
using RealEstate.Web.Models;

namespace RealEstate.Web.Services
{
    /// <summary>
    /// RealEstate.Api ile iletişim kuran HTTP client.
    /// Properties endpoint'inden emlak ilanlarını çeker.
    /// </summary>
    public class PropertyApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PropertyApiClient> _logger;

        public PropertyApiClient(HttpClient httpClient, ILogger<PropertyApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// API'den tüm emlak ilanlarını asenkron olarak getirir.
        /// Hata durumunda boş liste döner.
        /// </summary>
        /// <returns>Property listesi veya boş liste</returns>
        public async Task<List<Property>> GetFeaturedAsync()
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<List<Property>>("api/properties");
                _logger.LogInformation("API'den {Count} ilan getirildi.", result?.Count ?? 0);
                return result ?? new List<Property>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "API'ye bağlanırken hata oluştu. API çalışıyor mu kontrol et.");
                return new List<Property>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "İlanlar getirilirken beklenmeyen bir hata oluştu.");
                return new List<Property>();
            }
        }
    }
}