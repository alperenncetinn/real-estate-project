using System.Net.Http.Headers;
using System.Net.Http.Json; 
using Microsoft.Extensions.Configuration;
using RealEstate.Web.Models;
using System.Globalization;

namespace RealEstate.Web.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            var baseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:7180";
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        private void SetAuthHeader()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
            var token = _httpContextAccessor.HttpContext?.Request.Cookies["auth_token"];
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<List<ListingViewModel>> GetAllListingsAsync(string? type = null)
        {
            var url = "api/listings";
            if (!string.IsNullOrEmpty(type)) url += $"?type={type}";
            try { return await _httpClient.GetFromJsonAsync<List<ListingViewModel>>(url) ?? new List<ListingViewModel>(); }
            catch { return new List<ListingViewModel>(); }
        }

        public async Task<ListingViewModel> GetListingByIdAsync(int id)
        {
            try { return await _httpClient.GetFromJsonAsync<ListingViewModel>($"api/listings/{id}") ?? new ListingViewModel(); }
            catch { return new ListingViewModel(); }
        }

        // --- GÜNCELLENEN KISIM: Hatayı yakalayıp detay veriyoruz ---
        public async Task<(bool Success, string Error)> CreateListingAsync(CreateListingViewModel model)
        {
            try 
            {
                SetAuthHeader();
                using var content = new MultipartFormDataContent();
                var culture = CultureInfo.InvariantCulture;

                content.Add(new StringContent(model.Title ?? ""), "Title");
                content.Add(new StringContent(model.Description ?? ""), "Description");
                content.Add(new StringContent(model.Price.ToString(culture)), "Price");
                content.Add(new StringContent(model.City ?? ""), "City");
                content.Add(new StringContent(model.Type ?? "Satılık"), "Type");
                content.Add(new StringContent(model.RoomCount ?? ""), "RoomCount");
                content.Add(new StringContent(model.SquareMeters.ToString(culture)), "SquareMeters");
                content.Add(new StringContent(model.UserId.ToString()), "UserId");

                if (model.Photo != null)
                {
                    var fileContent = new StreamContent(model.Photo.OpenReadStream());
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(model.Photo.ContentType);
                    content.Add(fileContent, "Photo", model.Photo.FileName);
                }

                var response = await _httpClient.PostAsync("api/listings", content);

                if (response.IsSuccessStatusCode)
                {
                    return (true, "");
                }
                else
                {
                    // API cevabı boşsa Durum Kodunu (401, 500 vb.) yazdır
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(errorMsg))
                    {
                        return (false, $"Sunucu Cevabı Boş. Hata Kodu: {response.StatusCode} ({response.ReasonPhrase})");
                    }
                    return (false, errorMsg);
                }
            }
            catch (Exception ex)
            {
                // API kapalıysa veya ulaşılamıyorsa buraya düşer
                return (false, $"Bağlantı Hatası: {ex.Message}");
            }
        }

        public async Task<bool> UpdateListingAsync(int id, CreateListingViewModel model)
        {
            SetAuthHeader();
            using var content = new MultipartFormDataContent();
            var culture = CultureInfo.InvariantCulture;

            content.Add(new StringContent(model.Title ?? ""), "Title");
            content.Add(new StringContent(model.Description ?? ""), "Description");
            content.Add(new StringContent(model.Price.ToString(culture)), "Price");
            content.Add(new StringContent(model.City ?? ""), "City");
            content.Add(new StringContent(model.Type ?? "Satılık"), "Type");
            content.Add(new StringContent(model.RoomCount ?? ""), "RoomCount");
            content.Add(new StringContent(model.SquareMeters.ToString(culture)), "SquareMeters");
            content.Add(new StringContent(model.UserId.ToString()), "UserId");

            if (model.Photo != null)
            {
                var fileContent = new StreamContent(model.Photo.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(model.Photo.ContentType);
                content.Add(fileContent, "Photo", model.Photo.FileName);
            }

            var response = await _httpClient.PutAsync($"api/listings/{id}", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteListingAsync(int id)
        {
            SetAuthHeader();
            var response = await _httpClient.DeleteAsync($"api/listings/{id}");
            return (response.IsSuccessStatusCode, null);
        }

        public async Task<List<ListingViewModel>> GetMyListingsAsync()
        {
            SetAuthHeader();
            try { return await _httpClient.GetFromJsonAsync<List<ListingViewModel>>("api/listings/my") ?? new List<ListingViewModel>(); }
            catch { return new List<ListingViewModel>(); }
        }
    }
}