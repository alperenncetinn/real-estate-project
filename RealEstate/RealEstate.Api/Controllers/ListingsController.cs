using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RealEstate.Api.Data;
using RealEstate.Api.Dtos;
using RealEstate.Api.Entities;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
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

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
                return userId;
            return null;
        }

        private bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }

        // 1. LİSTELEME (GET) - Sadece aktif ilanlar (admin hariç)
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? type = null, [FromQuery] bool includeInactive = false)
        {
            var query = _context.Listings.Include(l => l.Owner).AsQueryable();

            // Admin değilse sadece aktif ilanları göster
            if (!IsAdmin() || !includeInactive)
            {
                query = query.Where(x => x.IsActive);
            }

            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(x => x.Type == type);
            }

            var listings = await query.Select(l => new
            {
                l.Id,
                l.Title,
                l.Description,
                l.Price,
                l.Type,
                l.City,
                l.District,
                l.ImageUrl,
                l.CreatedDate,
                l.IsActive,
                l.UserId,
                OwnerName = l.Owner != null ? l.Owner.FirstName + " " + l.Owner.LastName : null,
                l.DeactivationReason,
                l.DeactivatedAt
            }).ToListAsync();

            return Ok(listings);
        }

        // Kullanıcının kendi ilanları (pasif dahil)
        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyListings()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var listings = await _context.Listings
                .Where(l => l.UserId == userId)
                .Select(l => new
                {
                    l.Id,
                    l.Title,
                    l.Description,
                    l.Price,
                    l.Type,
                    l.City,
                    l.District,
                    l.ImageUrl,
                    l.CreatedDate,
                    l.IsActive,
                    l.DeactivationReason,
                    l.DeactivatedAt
                })
                .ToListAsync();

            return Ok(listings);
        }

        // 2. ID İLE GETİRME (GET BY ID)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var listing = await _context.Listings
                .Include(l => l.Owner)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (listing == null)
            {
                return NotFound("Ilan bulunamadi.");
            }

            // Pasif ilan kontrolü - sadece sahibi veya admin görebilir
            if (!listing.IsActive)
            {
                var currentUserId = GetCurrentUserId();
                if (!IsAdmin() && listing.UserId != currentUserId)
                {
                    return NotFound("Ilan bulunamadi.");
                }
            }

            return Ok(new
            {
                listing.Id,
                listing.Title,
                listing.Description,
                listing.Price,
                listing.Type,
                listing.City,
                listing.District,
                listing.ImageUrl,
                listing.CreatedDate,
                listing.IsActive,
                listing.UserId,
                OwnerName = listing.Owner != null ? listing.Owner.FirstName + " " + listing.Owner.LastName : null,
                listing.DeactivationReason,
                listing.DeactivatedAt
            });
        }

        // 3. EKLEME (POST)
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromForm] ListingDtos dto)
        {
            if (dto == null) return BadRequest("Veri gelmedi.");

            var userId = GetCurrentUserId();

            var listing = new Listing
            {
                Title = dto.Title,
                Description = dto.Description,
                City = dto.City,
                Price = dto.Price,
                Type = dto.Type,
                CreatedDate = DateTime.UtcNow,
                UserId = userId,
                IsActive = true
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

            return Ok(new { message = "Ilan olusturuldu", data = listing });
        }

        // 4. GÜNCELLEME (PUT) - Sadece ilan sahibi
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromForm] ListingDtos dto)
        {
            var existingListing = await _context.Listings.FindAsync(id);
            if (existingListing == null) return NotFound();

            var currentUserId = GetCurrentUserId();

            // Sadece ilan sahibi güncelleyebilir
            if (existingListing.UserId != currentUserId)
            {
                return Forbid("Bu ilani duzenleme yetkiniz yok.");
            }

            // Pasif ilan güncellenemez
            if (!existingListing.IsActive)
            {
                return BadRequest(new { message = "Pasif ilanlar duzenlenemez." });
            }

            existingListing.Title = dto.Title;
            existingListing.Description = dto.Description;
            existingListing.City = dto.City;
            existingListing.Price = dto.Price;
            existingListing.Type = dto.Type;

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

            await _context.SaveChangesAsync();

            return Ok(new { message = $"ID'si {id} olan ilan guncellendi.", data = existingListing });
        }

        // 5. PASİFE ÇEKME (Admin Only)
        [HttpPut("{id}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Deactivate(int id, [FromBody] DeactivateListingDto dto)
        {
            var listing = await _context.Listings.FindAsync(id);
            if (listing == null) return NotFound();

            if (!listing.IsActive)
            {
                return BadRequest(new { message = "Ilan zaten pasif." });
            }

            var adminUserId = GetCurrentUserId();

            listing.IsActive = false;
            listing.DeactivationReason = dto?.Reason ?? "Admin tarafindan pasife alindi.";
            listing.DeactivatedAt = DateTime.UtcNow;
            listing.DeactivatedByUserId = adminUserId;

            // Bildirim oluştur (ilan sahibine)
            if (listing.UserId.HasValue)
            {
                var notification = new Notification
                {
                    UserId = listing.UserId.Value,
                    Title = "Ilaniniz Pasife Alindi",
                    Message = $"'{listing.Title}' baslikli ilaniniz admin tarafindan pasife alindi. Neden: {listing.DeactivationReason}",
                    Type = "warning",
                    ListingId = listing.Id,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };
                _context.Notifications.Add(notification);
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Ilan pasife alindi ve kullaniciya bildirim gonderildi." });
        }

        // 6. AKTİFE ALMA (Admin Only)
        [HttpPut("{id}/activate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Activate(int id)
        {
            var listing = await _context.Listings.FindAsync(id);
            if (listing == null) return NotFound();

            if (listing.IsActive)
            {
                return BadRequest(new { message = "Ilan zaten aktif." });
            }

            listing.IsActive = true;
            listing.DeactivationReason = null;
            listing.DeactivatedAt = null;
            listing.DeactivatedByUserId = null;

            // Bildirim oluştur (ilan sahibine)
            if (listing.UserId.HasValue)
            {
                var notification = new Notification
                {
                    UserId = listing.UserId.Value,
                    Title = "Ilaniniz Tekrar Aktif",
                    Message = $"'{listing.Title}' baslikli ilaniniz admin tarafindan tekrar aktif edildi.",
                    Type = "success",
                    ListingId = listing.Id,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };
                _context.Notifications.Add(notification);
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Ilan aktif edildi." });
        }

        // 7. SİLME (DELETE) - Sadece ilan sahibi veya admin
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var listing = await _context.Listings.FindAsync(id);
            if (listing == null) return NotFound();

            var currentUserId = GetCurrentUserId();

            // Sadece ilan sahibi veya admin silebilir
            if (!IsAdmin() && listing.UserId != currentUserId)
            {
                return Forbid("Bu ilani silme yetkiniz yok.");
            }

            _context.Listings.Remove(listing);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"ID'si {id} olan ilan silindi." });
        }
    }

    public class DeactivateListingDto
    {
        public string? Reason { get; set; }
    }
}