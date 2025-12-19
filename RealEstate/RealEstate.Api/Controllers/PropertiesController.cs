using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstate.Api.Data;
using RealEstate.Api.Models;

namespace RealEstate.Api.Controllers
{
    /// <summary>
    /// Emlak ilanlarını yönetir ve API endpoint'leri sağlar.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PropertiesController : ControllerBase
    {
        private readonly ILogger<PropertiesController> _logger;
        private readonly ApplicationDbContext _context;

        public PropertiesController(ILogger<PropertiesController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Tüm emlak ilanlarını getirir.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Property>>> GetAll()
        {
            var properties = await _context.Properties.ToListAsync();
            _logger.LogInformation("Toplam {Count} ilan getirildi.", properties.Count);
            return Ok(properties);
        }

        /// <summary>
        /// ID ile tek bir ilan getirir.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Property>> GetById(int id)
        {
            var property = await _context.Properties.FindAsync(id);
            if (property == null)
            {
                return NotFound("İlan bulunamadı.");
            }
            return Ok(property);
        }

        /// <summary>
        /// Yeni ilan ekler.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<Property>> Create([FromForm] PropertyCreateDto dto)
        {
            var property = new Property
            {
                Title = dto.Title ?? "Başlıksız İlan",
                City = dto.City ?? "Belirtilmedi",
                District = dto.District ?? "",
                Price = dto.Price,
                RoomCount = ParseRoomCount(dto.RoomCount),
                Area = dto.SquareMeters,
                ImageUrl = "https://placehold.co/600x400?text=Yeni+Ilan"
            };

            _context.Properties.Add(property);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Yeni ilan eklendi: {Title}", property.Title);

            return CreatedAtAction(nameof(GetById), new { id = property.Id }, property);
        }

        /// <summary>
        /// Mevcut bir ilanı günceller.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Property>> Update(int id, [FromForm] PropertyCreateDto dto)
        {
            var property = await _context.Properties.FindAsync(id);
            if (property == null)
            {
                return NotFound("İlan bulunamadı.");
            }

            property.Title = dto.Title ?? property.Title;
            property.City = dto.City ?? property.City;
            property.District = dto.District ?? property.District;
            property.Price = dto.Price;
            property.RoomCount = ParseRoomCount(dto.RoomCount);
            property.Area = dto.SquareMeters;

            await _context.SaveChangesAsync();

            _logger.LogInformation("İlan güncellendi: {Title}", property.Title);

            return Ok(property);
        }

        /// <summary>
        /// Bir ilanı siler.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var property = await _context.Properties.FindAsync(id);
            if (property == null)
            {
                return NotFound("İlan bulunamadı.");
            }

            _context.Properties.Remove(property);
            await _context.SaveChangesAsync();

            _logger.LogInformation("İlan silindi: {Id}", id);

            return Ok(new { message = $"ID'si {id} olan ilan silindi." });
        }

        private int ParseRoomCount(string? roomCount)
        {
            if (string.IsNullOrEmpty(roomCount)) return 1;
            // "3+1" -> 3, "2+1" -> 2 şeklinde parse et
            var parts = roomCount.Split('+');
            if (int.TryParse(parts[0], out int count))
                return count;
            return 1;
        }
    }

    public class PropertyCreateDto
    {
        public string? Title { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public decimal Price { get; set; }
        public string? RoomCount { get; set; }
        public int SquareMeters { get; set; }
        public string? Description { get; set; }
    }
}