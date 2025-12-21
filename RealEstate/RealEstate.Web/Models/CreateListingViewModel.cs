using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace RealEstate.Web.Models
{
    public class CreateListingViewModel
    {
        public int UserId { get; set; }

        public string? Title { get; set; } 

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public string? City { get; set; }

        public string? Type { get; set; }
        
        public string? RoomCount { get; set; }

        public int SquareMeters { get; set; }

        // SORU İŞARETİ (?) İLE OPSİYONEL YAPTIK
        public IFormFile? Photo { get; set; }
        
        public string? ImageUrl { get; set; }
    }
}