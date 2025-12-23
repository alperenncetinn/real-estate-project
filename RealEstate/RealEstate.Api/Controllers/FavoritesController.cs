using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstate.Api.Data;
using RealEstate.Api.Entities;
using System.Security.Claims;

namespace RealEstate.Api.Controllers
{
    /// <summary>
    /// Kullanıcının favori ilanlarını yönetir.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FavoritesController : ControllerBase
    {
        private readonly ILogger<FavoritesController> _logger;
        private readonly ApplicationDbContext _context;

        public FavoritesController(ILogger<FavoritesController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Giriş yapan kullanıcının favorilerini getirir.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<Favorite>>> GetMyFavorites()
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized("Geçersiz token.");

            var favorites = await _context.Favorites
                .Where(f => f.UserId == userId.Value)
                .Include(f => f.Listing)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            _logger.LogInformation("UserId={UserId} için {Count} favori getirildi.", userId.Value, favorites.Count);

            return Ok(favorites);
        }

        /// <summary>
        /// Bir ilanı favorilere ekler.
        /// </summary>
        [HttpPost("{listingId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddToFavorites(int listingId)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized("Geçersiz token.");

            // İlan var mı?
            var listingExists = await _context.Listings.AnyAsync(l => l.Id == listingId);
            if (!listingExists)
                return NotFound("İlan bulunamadı.");

            // Zaten favoride mi?
            var exists = await _context.Favorites.AnyAsync(f => f.UserId == userId.Value && f.ListingId == listingId);
            if (exists)
                return Ok(new { message = "Bu ilan zaten favorilerinizde." });

            var fav = new Favorite
            {
                UserId = userId.Value,
                ListingId = listingId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Favorites.Add(fav);
            await _context.SaveChangesAsync();

            _logger.LogInformation("UserId={UserId}, ListingId={ListingId} favorilere eklendi.", userId.Value, listingId);

            return Ok(new { message = "Favorilere eklendi." });
        }

        /// <summary>
        /// Bir ilanı favorilerden çıkarır.
        /// </summary>
        [HttpDelete("{listingId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RemoveFromFavorites(int listingId)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized("Geçersiz token.");

            var fav = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId.Value && f.ListingId == listingId);

            if (fav == null)
                return Ok(new { message = "Favorilerde yoktu." });

            _context.Favorites.Remove(fav);
            await _context.SaveChangesAsync();

            _logger.LogInformation("UserId={UserId}, ListingId={ListingId} favorilerden çıkarıldı.", userId.Value, listingId);

            return Ok(new { message = "Favorilerden çıkarıldı." });
        }

        private int? GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return null;

            return userId;
        }
    }
}
