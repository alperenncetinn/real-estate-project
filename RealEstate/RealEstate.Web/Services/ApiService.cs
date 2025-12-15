using System.Net.Http.Headers;
using RealEstate.Web.Models;

namespace RealEstate.Web.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            // TODO: Move BaseAddress to appsettings.json for production
            _httpClient.BaseAddress = new Uri("http://localhost:5180/");
        }

        public async Task<bool> CreateListingAsync(CreateListingViewModel model)
        {
            using var content = new MultipartFormDataContent();

            content.Add(new StringContent(model.Title ?? string.Empty), "Title");
            content.Add(new StringContent(model.City ?? string.Empty), "City");
            content.Add(new StringContent(model.Price.ToString()), "Price");
            content.Add(new StringContent(model.RoomCount ?? string.Empty), "RoomCount");
            content.Add(new StringContent(model.SquareMeters.ToString()), "SquareMeters");
            content.Add(new StringContent(model.Description ?? string.Empty), "Description");

            if (model.Photo != null)
            {
                var fileContent = new StreamContent(model.Photo.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(model.Photo.ContentType);
                content.Add(fileContent, "Photo", model.Photo.FileName);
            }

            var response = await _httpClient.PostAsync("api/properties", content);

            return response.IsSuccessStatusCode;
        }
    }
}