using Microsoft.AspNetCore.Mvc;
using RealEstate.Api.Dtos;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace RealEstate.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ListingsController : ControllerBase
    {
        // RAM'deki Sahte Veritabanı
        private static List<ListingDtos> _listings = new List<ListingDtos>();

        private readonly IWebHostEnvironment _environment;

        public ListingsController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        // 1. LİSTELEME (GET)
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_listings);
        }

        // 2. ID İLE GETİRME (GET BY ID) - Düzenleme sayfası için gerekli
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            if (id < 0 || id >= _listings.Count)
            {
                return NotFound("İlan bulunamadı.");
            }
            return Ok(_listings[id]);
        }

        // 3. EKLEME (POST)
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ListingDtos dto)
        {
            if (dto == null) return BadRequest("Veri gelmedi.");

            // FOTOĞRAF YÜKLEME
            if (dto.Photo != null && dto.Photo.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.Photo.FileName);
                var uploadPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads");

                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Photo.CopyToAsync(stream);
                }
                dto.ImageUrl = "/uploads/" + fileName;
            }

            _listings.Add(dto);
            return Ok(new { message = "İlan oluşturuldu", data = dto });
        }

        // 4. GÜNCELLEME (PUT) - BU KISIM GÜNCELLENDİ
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] ListingDtos dto)
        {
            // İlan var mı kontrol et
            if (id < 0 || id >= _listings.Count) return NotFound();

            // Eski ilanı al (Eski resim yolunu kaybetmemek için)
            var existingListing = _listings[id];

            // --- YENİ FOTOĞRAF VAR MI? ---
            if (dto.Photo != null && dto.Photo.Length > 0)
            {
                // Varsa yeni fotoğrafı kaydet (Create ile aynı mantık)
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.Photo.FileName);
                var uploadPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads");

                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Photo.CopyToAsync(stream);
                }
                
                // Yeni resim yolunu ata
                dto.ImageUrl = "/uploads/" + fileName;
            }
            else
            {
                // Yeni fotoğraf yoksa, ESKİ FOTOĞRAFI KORU
                dto.ImageUrl = existingListing.ImageUrl;
            }
            // -----------------------------

            // Listeyi güncelle
            _listings[id] = dto;

            return Ok(new { message = $"ID'si {id} olan ilan güncellendi.", data = dto });
        }

        // 5. SİLME (DELETE)
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (id < 0 || id >= _listings.Count) return NotFound();

            _listings.RemoveAt(id);

            return Ok(new { message = $"ID'si {id} olan ilan silindi." });
        }
    }
}