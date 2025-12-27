namespace RealEstate.Api.Dtos
{
    public class ImageCreateDto
    {
        public int ListingId { get; set; }
        public string Base64Data { get; set; }
        public string? FileName { get; set; }
    }

    public class ImageDto
    {
        public int Id { get; set; }
        public int ListingId { get; set; }
        public string Base64Data { get; set; }
        public string? FileName { get; set; }
    }
}
