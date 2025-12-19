using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using RealEstate.Web.Models;

namespace RealEstate.Web.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            var baseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5180";
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        // --- 1. LİSTELEME (Tümünü Getir) ---
        // GÜNCELLENDİ: 'type' parametresi alarak filtreleme yapıyor
        public async Task<List<ListingViewModel>> GetAllListingsAsync(string? type = null)
        {
            // URL'yi dinamik oluşturuyoruz
            var url = "api/listings";
            
            // Eğer bir tür seçildiyse (Kiralık/Satılık), URL'nin sonuna ekle
            if (!string.IsNullOrEmpty(type))
            {
                url += $"?type={type}";
            }

            // Eğer null gelirse boş liste ver
            return await _httpClient.GetFromJsonAsync<List<ListingViewModel>>(url) ?? new List<ListingViewModel>();
        }

        // --- 2. TEK İLAN GETİR (Düzenleme Sayfası İçin) ---
        public async Task<ListingViewModel> GetListingByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<ListingViewModel>($"api/listings/{id}") ?? new ListingViewModel();
        }

        // --- 3. EKLEME (POST) ---
        public async Task<bool> CreateListingAsync(CreateListingViewModel model)
        {
            using var content = new MultipartFormDataContent();

            content.Add(new StringContent(model.Title ?? string.Empty), "Title");
            content.Add(new StringContent(model.City ?? string.Empty), "City");
            content.Add(new StringContent(model.Price.ToString()), "Price");

            // --- YENİ EKLENEN: Type (Satılık/Kiralık) bilgisini gönderiyoruz ---
            content.Add(new StringContent(model.Type ?? string.Empty), "Type");
            // ------------------------------------------------------------------

            // DTO'da varsa ekle, yoksa boş geç
            content.Add(new StringContent(model.RoomCount ?? string.Empty), "RoomCount");
            content.Add(new StringContent(model.SquareMeters.ToString()), "SquareMeters");
            content.Add(new StringContent(model.Description ?? string.Empty), "Description");

            if (model.Photo != null)
            {
                var fileContent = new StreamContent(model.Photo.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(model.Photo.ContentType);
                content.Add(fileContent, "Photo", model.Photo.FileName);
            }

            var response = await _httpClient.PostAsync("api/listings", content);

            return response.IsSuccessStatusCode;
        }

        // --- 4. GÜNCELLEME (PUT) ---
        public async Task<bool> UpdateListingAsync(int id, CreateListingViewModel model)
        {
            using var content = new MultipartFormDataContent();

            content.Add(new StringContent(model.Title ?? string.Empty), "Title");
            content.Add(new StringContent(model.City ?? string.Empty), "City");
            content.Add(new StringContent(model.Price.ToString()), "Price");
            
            // --- YENİ EKLENEN: Type (Satılık/Kiralık) bilgisini gönderiyoruz ---
            content.Add(new StringContent(model.Type ?? string.Empty), "Type");
            // ------------------------------------------------------------------

            content.Add(new StringContent(model.Description ?? string.Empty), "Description");

            if (model.Photo != null)
            {
                var fileContent = new StreamContent(model.Photo.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(model.Photo.ContentType);
                content.Add(fileContent, "Photo", model.Photo.FileName);
            }

            // PUT isteği gönderiyoruz
            var response = await _httpClient.PutAsync($"api/listings/{id}", content);

            return response.IsSuccessStatusCode;
        }

        // --- 5. SİLME (DELETE) ---
        public async Task<bool> DeleteListingAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/listings/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Hatası: {ex.Message}");
                return false;
            }
        }
    }
}