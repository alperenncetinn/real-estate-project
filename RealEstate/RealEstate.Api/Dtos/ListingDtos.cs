using Microsoft.AspNetCore.Http;

namespace RealEstate.Api.Dtos
{
    public class ListingDtos
    {
        public string? Title { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public decimal Price { get; set; }
        public string? Type { get; set; }
        public string? RoomCount { get; set; }
        public int SquareMeters { get; set; }
        public string? Description { get; set; }
        public IFormFile? Photo { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class ListingResponseDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public decimal Price { get; set; }
        public string? Type { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public string? RoomCount { get; set; }
        public string? Description { get; set; }
        public int SquareMeters { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedDate { get; set; }

        // Ownership & Status
        public int? UserId { get; set; }
        public bool IsActive { get; set; }
        public string? OwnerName { get; set; }
        public string? OwnerPhone { get; set; }
        public string? DeactivationReason { get; set; }
        public DateTime? DeactivatedAt { get; set; }
    }
}