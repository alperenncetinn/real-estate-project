using Microsoft.AspNetCore.Mvc;
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

        // In-memory veri deposu (uygulama yeniden başlatılana kadar kalıcı)
        private static List<Property> _properties = new List<Property>
        {
            new Property
            {
                Id = 1,
                Title = "Merkezde 2+1 Daire",
                City = "Manisa",
                District = "Yunusemre",
                Price = 2_500_000,
                RoomCount = 2,
                Area = 110,
                ImageUrl = "https://placehold.co/600x400?text=House+1"
            },
            new Property
            {
                Id = 2,
                Title = "Site İçinde 3+1 Daire",
                City = "İzmir",
                District = "Bornova",
                Price = 4_100_000,
                RoomCount = 3,
                Area = 140,
                ImageUrl = "https://placehold.co/600x400?text=House+2"
            }
        };

        private static int _nextId = 3;

        public PropertiesController(ILogger<PropertiesController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Tüm emlak ilanlarını getirir.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<Property>> GetAll()
        {
            _logger.LogInformation("Toplam {Count} ilan getirildi.", _properties.Count);
            return Ok(_properties);
        }

        /// <summary>
        /// Yeni ilan ekler.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public ActionResult<Property> Create([FromForm] PropertyCreateDto dto)
        {
            var property = new Property
            {
                Id = _nextId++,
                Title = dto.Title ?? "Başlıksız İlan",
                City = dto.City ?? "Belirtilmedi",
                District = dto.District ?? "",
                Price = dto.Price,
                RoomCount = ParseRoomCount(dto.RoomCount),
                Area = dto.SquareMeters,
                ImageUrl = "https://placehold.co/600x400?text=Yeni+Ilan"
            };

            _properties.Add(property);
            _logger.LogInformation("Yeni ilan eklendi: {Title}", property.Title);

            return CreatedAtAction(nameof(GetAll), new { id = property.Id }, property);
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