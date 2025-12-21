using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstate.Api.Entities
{
    public class Listing
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; } // Zorunlu alan (int)

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string City { get; set; } = string.Empty;
        public string? District { get; set; }
        public string Type { get; set; } = "Satılık";

        // --- YENİ EKLENEN ALANLAR ---
        public string? RoomCount { get; set; } 
        public int SquareMeters { get; set; }
        // ----------------------------

        public string? ImageUrl { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Pasife alma detayları
        public string? DeactivationReason { get; set; }
        public DateTime? DeactivatedAt { get; set; }
        public int? DeactivatedByUserId { get; set; }

        // --- İLİŞKİLER (Navigation Properties) ---

        [ForeignKey("UserId")]
        public virtual User? Owner { get; set; }

        // HATA VEREN KISIM BURASIYDI, BUNU EKLEDİK:
        [ForeignKey("DeactivatedByUserId")]
        public virtual User? DeactivatedBy { get; set; }
    }
}