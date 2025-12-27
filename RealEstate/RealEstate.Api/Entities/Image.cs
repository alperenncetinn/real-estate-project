using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstate.Api.Entities
{
    [Table("images")]
    public class Image
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("listing_id")]
        public int ListingId { get; set; }

        // Navigation property opsiyonel
        public Listing? Listing { get; set; }

        [Required]
        [Column("base64data")]
        public string Base64Data { get; set; }

        [Column("filename")]
        public string? FileName { get; set; }
    }
}