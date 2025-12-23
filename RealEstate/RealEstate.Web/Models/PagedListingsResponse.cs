using System.Text.Json.Serialization;

namespace RealEstate.Web.Models
{
    public class PagedListingsResponse
    {
        [JsonPropertyName("items")]
        public List<ListingViewModel> Items { get; set; } = new List<ListingViewModel>();

        [JsonPropertyName("pageNumber")]
        public int PageNumber { get; set; }

        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }

        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }

        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; }
    }
}
