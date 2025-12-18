using Microsoft.AspNetCore.Http;

namespace RealEstate.Web.Models
{
    public class CreateListingViewModel
    {
        public string? Title { get; set; }
        public string? City { get; set; }
        public decimal Price { get; set; }
        public string? RoomCount { get; set; }
        public int SquareMeters { get; set; }
        public string? Description { get; set; }
        public IFormFile? Photo { get; set; }
    }
}