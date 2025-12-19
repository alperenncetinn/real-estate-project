using System;
using System.ComponentModel.DataAnnotations;

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
    }
}