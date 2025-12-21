using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RealEstate.Api.Dtos;
using RealEstate.Api.Services;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RealEstate.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ListingsController : ControllerBase
    {
        private readonly IListingService _listingService;

        public ListingsController(IListingService listingService)
        {
            _listingService = listingService;
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

        // 1. LİSTELEME
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? type = null, [FromQuery] bool includeInactive = false)
        {
            var listings = await _listingService.GetAllAsync(type, includeInactive, IsAdmin());
            return Ok(listings);
        }

        // KULLANICININ İLANLARI
        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyListings()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var listings = await _listingService.GetMyListingsAsync(userId.Value);
            return Ok(listings);
        }

        // 2. ID İLE GETİRME
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _listingService.GetByIdAsync(id, GetCurrentUserId(), IsAdmin());
            if (!result.Success) return NotFound(result.Error);

            return Ok(result.Data);
        }

        // 3. EKLEME
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromForm] ListingDtos dto)
        {
            Console.WriteLine("[API CREATE] Request geldi...");
            Console.WriteLine($"[API CREATE] User Claims: {string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}"))}");

            if (dto == null) return BadRequest("Veri gelmedi.");

            var userId = GetCurrentUserId();
            Console.WriteLine($"[API CREATE] GetCurrentUserId Sonucu: {userId}");

            if (userId == null)
            {
                Console.WriteLine("[API CREATE] Unauthorized - UserId null");
                return Unauthorized();
            }

            var result = await _listingService.CreateAsync(dto, userId.Value);
            if (!result.Success) return BadRequest(result.Error);

            return Ok(new { message = "Ilan olusturuldu", data = result.Data });
        }

        // 4. GÜNCELLEME
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromForm] ListingDtos dto)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null) return Unauthorized();

            var result = await _listingService.UpdateAsync(id, dto, currentUserId.Value);
            if (!result.Success)
            {
                return result.ErrorType switch
                {
                    OperationErrorType.NotFound => NotFound(result.Error),
                    OperationErrorType.Forbidden => Forbid(result.Error),
                    _ => BadRequest(new { message = result.Error ?? "Guncellenemedi." })
                };
            }

            return Ok(new { message = "Guncellendi." });
        }

        // 5. PASİFE ÇEKME (HATA BURADAYDI - DÜZELTİLDİ)
        [HttpPut("{id}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Deactivate(int id, [FromBody] DeactivateListingDto dto)
        {
            var adminUserId = GetCurrentUserId();
            if (adminUserId == null) return Unauthorized();

            var result = await _listingService.DeactivateAsync(id, adminUserId.Value, dto?.Reason);
            if (!result.Success)
            {
                return result.ErrorType == OperationErrorType.NotFound
                    ? NotFound(result.Error)
                    : BadRequest(new { message = result.Error ?? "Pasife alinami̇yor." });
            }

            return Ok(new { message = "Pasife alindi." });
        }

        // 6. AKTİFE ALMA (HATA BURADAYDI - DÜZELTİLDİ)
        [HttpPut("{id}/activate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Activate(int id)
        {
            var adminUserId = GetCurrentUserId();
            if (adminUserId == null) return Unauthorized();

            var result = await _listingService.ActivateAsync(id, adminUserId.Value);
            if (!result.Success)
            {
                return result.ErrorType == OperationErrorType.NotFound
                    ? NotFound(result.Error)
                    : BadRequest(new { message = result.Error ?? "Aktif edilemedi." });
            }

            return Ok(new { message = "Aktif edildi." });
        }

        // 7. SİLME
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null && !IsAdmin()) return Unauthorized();

            var result = await _listingService.DeleteAsync(id, currentUserId ?? 0, IsAdmin());
            if (!result.Success)
            {
                return result.ErrorType switch
                {
                    OperationErrorType.NotFound => NotFound(result.Error),
                    OperationErrorType.Forbidden => Forbid(result.Error),
                    _ => BadRequest(new { message = result.Error ?? "Silinemedi." })
                };
            }

            return Ok(new { message = "Silindi." });
        }
    }

    public class DeactivateListingDto
    {
        public string? Reason { get; set; }
    }
}