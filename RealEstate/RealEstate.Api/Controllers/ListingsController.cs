using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstate.Api.Data;
using RealEstate.Api.Dtos;
using RealEstate.Api.Entities;
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
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ListingsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // 1. LİSTELEME (GET)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var listings = await _context.Listings.ToListAsync();
            return Ok(listings);
        }

        // 2. ID İLE GETİRME (GET BY ID) - Düzenleme sayfası için gerekli
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var listing = await _context.Listings.FindAsync(id);
            if (listing == null)
            {
                return NotFound("İlan bulunamadı.");
            }
            return Ok(listing);
        }

        // 3. EKLEME (POST)
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ListingDtos dto)
        {
            if (dto == null) return BadRequest("Veri gelmedi.");

            var listing = new Listing
            {
                Title = dto.Title,
                Description = dto.Description,
                City = dto.City,
                Price = dto.Price,
                CreatedDate = DateTime.UtcNow
            };

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
                listing.ImageUrl = "/uploads/" + fileName;
            }

            _context.Listings.Add(listing);
            await _context.SaveChangesAsync();

            return Ok(new { message = "İlan oluşturuldu", data = listing });
        }

        // 4. GÜNCELLEME (PUT)
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] ListingDtos dto)
        {
            var existingListing = await _context.Listings.FindAsync(id);
            if (existingListing == null) return NotFound();

            // Alanları güncelle
            existingListing.Title = dto.Title;
            existingListing.Description = dto.Description;
            existingListing.City = dto.City;
            existingListing.Price = dto.Price;

            // --- YENİ FOTOĞRAF VAR MI? ---
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

                existingListing.ImageUrl = "/uploads/" + fileName;
            }
            // Yeni fotoğraf yoksa, ESKİ FOTOĞRAF KORUNUR

            await _context.SaveChangesAsync();

            return Ok(new { message = $"ID'si {id} olan ilan güncellendi.", data = existingListing });
        }

        // 5. SİLME (DELETE)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var listing = await _context.Listings.FindAsync(id);
            if (listing == null) return NotFound();

            _context.Listings.Remove(listing);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"ID'si {id} olan ilan silindi." });
        }
    }
}