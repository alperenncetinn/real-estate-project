using Microsoft.AspNetCore.Http;

namespace RealEstate.Api.Dtos
{
    public class ListingDtos
    {
        public string? Title { get; set; }
        public string? City { get; set; }
        public decimal Price { get; set; }
        public string? RoomCount { get; set; }
        public int SquareMeters { get; set; }
        public string? Description { get; set; }
        public IFormFile? Photo { get; set; }
        public string? ImageUrl { get; set; }
    }
}