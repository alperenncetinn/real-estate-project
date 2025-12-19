namespace RealEstate.Web.Models
{
    public class ListingViewModel
    {
        // API'deki ListingDtos ile birebir aynı isimde olmalı ki veriler eşleşsin
        public int Id { get; set; }
        public string? Title { get; set; }
        public decimal Price { get; set; }
        public string? Type { get; set; }
        public string? City { get; set; }
        public string? RoomCount { get; set; }
        public string? Description { get; set; }
        public int SquareMeters { get; set; }
        public string? ImageUrl { get; set; } // Resim yolunu gösterebilmek için
    }
}