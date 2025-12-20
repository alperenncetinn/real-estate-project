using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using RealEstate.Web.Models;

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
            var baseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5180";
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        private void SetAuthHeader()
        {
            // Önce mevcut Authorization header'ı temizle
            _httpClient.DefaultRequestHeaders.Authorization = null;

            var token = _httpContextAccessor.HttpContext?.Request.Cookies["auth_token"];
            Console.WriteLine($"[ApiService] Token from cookie: {(string.IsNullOrEmpty(token) ? "NULL/EMPTY" : token.Substring(0, Math.Min(20, token.Length)) + "...")}");

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                Console.WriteLine("[ApiService] Authorization header set successfully");
            }
            else
            {
                Console.WriteLine("[ApiService] WARNING: No token found in cookies!");
            }
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
            SetAuthHeader();

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
            SetAuthHeader();

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
        public async Task<(bool Success, string? ErrorMessage)> DeleteListingAsync(int id)
        {
            try
            {
                SetAuthHeader();
                var response = await _httpClient.DeleteAsync($"api/listings/{id}");

                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return (false, "Bu ilani silme yetkiniz yok.");
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return (false, "Ilan bulunamadi.");
                }

                return (false, "Ilan silinirken bir hata olustu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Hatası: {ex.Message}");
                return (false, "Bir hata olustu. Lutfen tekrar deneyin.");
            }
        }

        // --- 6. KULLANICININ İLANLARI ---
        public async Task<List<ListingViewModel>> GetMyListingsAsync()
        {
            try
            {
                SetAuthHeader();
                return await _httpClient.GetFromJsonAsync<List<ListingViewModel>>("api/listings/my") ?? new List<ListingViewModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Hatası: {ex.Message}");
                return new List<ListingViewModel>();
            }
        }
    }
}