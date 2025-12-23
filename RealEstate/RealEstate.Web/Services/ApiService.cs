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
            var baseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5180";
            _httpClient.BaseAddress = new Uri(baseUrl);
            Console.WriteLine($"[API SERVICE] BaseAddress: {_httpClient.BaseAddress}");
        }

        private void SetAuthHeader()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
            var token = _httpContextAccessor.HttpContext?.Request.Cookies["auth_token"];

            Console.WriteLine($"[API DEBUG] Token Kontrol: {(string.IsNullOrEmpty(token) ? "BOŞ" : "VAR")}");
            if (!string.IsNullOrEmpty(token))
            {
                Console.WriteLine($"[API DEBUG] Token İlk 20 Karakter: {token.Substring(0, Math.Min(20, token.Length))}...");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                Console.WriteLine("[API DEBUG] Authorization Header Set edildi");
            }
            else
            {
                Console.WriteLine("[API DEBUG] Token alınamadı! Cookie bulunamadı veya boş.");
            }
        }

        public async Task<PaginationViewModel<ListingViewModel>> GetAllListingsAsync(string? type = null, int page = 1, int pageSize = 9)
        {
            var url = $"api/listings?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(type)) url += $"&type={type}";

            Console.WriteLine($"[ApiService] API'ye istek atılıyor: {url}");

            try
            {
                var result = await _httpClient.GetFromJsonAsync<PagedListingsResponse>(url);
                
                if (result == null || result.Items == null)
                {
                    Console.WriteLine("[ApiService] API null döndü");
                    return new PaginationViewModel<ListingViewModel>
                    {
                        Items = new List<ListingViewModel>(),
                        PageNumber = page,
                        PageSize = pageSize,
                        TotalCount = 0
                    };
                }

                var response = new PaginationViewModel<ListingViewModel>
                {
                    Items = result.Items,
                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize,
                    TotalCount = result.TotalCount
                };

                Console.WriteLine($"[ApiService] API'den {result.Items.Count} ilan döndü, Toplam: {response.TotalCount}, Sayfa: {page}/{response.TotalPages}");
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiService] API çağrısında hata: {ex.Message}\n{ex.StackTrace}");
                return new PaginationViewModel<ListingViewModel>
                {
                    Items = new List<ListingViewModel>(),
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalCount = 0
                };
            }
        }

        public async Task<List<ListingViewModel>> GetAllListingsAsync(string? type = null)
        {
            var result = await GetAllListingsAsync(type, 1, 9);
            return result.Items;
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
                Console.WriteLine("[CREATE LISTING] Başlatılıyor...");
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

                Console.WriteLine("[CREATE LISTING] API'ye istek gönderiliyor...");
                var response = await _httpClient.PostAsync("api/listings", content);

                Console.WriteLine($"[CREATE LISTING] Yanıt Kodu: {response.StatusCode} ({response.ReasonPhrase})");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("[CREATE LISTING] BAŞARILI");
                    return (true, "");
                }
                else
                {
                    // API cevabı boşsa Durum Kodunu (401, 500 vb.) yazdır
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[CREATE LISTING] Hata Mesajı: {errorMsg}");

                    if (string.IsNullOrEmpty(errorMsg))
                    {
                        return (false, $"Sunucu Cevabı Boş. Hata Kodu: {response.StatusCode} ({response.ReasonPhrase})");
                    }
                    return (false, errorMsg);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CREATE LISTING] Exception: {ex.Message}");
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