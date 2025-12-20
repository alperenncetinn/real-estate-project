using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstate.Api.Entities
{
    public class Listing
    {
        [Key]
        public int Id { get; set; }

        public string? Title { get; set; }       // İlan Başlığı
        public string? Description { get; set; } // Açıklama
        public decimal Price { get; set; }      // Fiyat
        public string? Type { get; set; }
        public string? City { get; set; }        // Şehir
        public string? District { get; set; }    // İlçe
        public string? ImageUrl { get; set; }    // Resim Yolu
        public DateTime CreatedDate { get; set; } // Oluşturulma Tarihi

        // Ownership & Status
        public int? UserId { get; set; }         // İlanı oluşturan kullanıcı
        public bool IsActive { get; set; } = true; // İlan aktif mi?
        public string? DeactivationReason { get; set; } // Pasife alınma nedeni
        public DateTime? DeactivatedAt { get; set; } // Pasife alınma tarihi
        public int? DeactivatedByUserId { get; set; } // Pasife alan admin

        [ForeignKey("UserId")]
        public virtual User? Owner { get; set; }

        [ForeignKey("DeactivatedByUserId")]
        public virtual User? DeactivatedBy { get; set; }
    }
}