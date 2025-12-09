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

        public PropertiesController(ILogger<PropertiesController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Tüm emlak ilanlarını getirir.
        /// </summary>
        /// <returns>Property listesi</returns>
        /// <response code="200">İlanlar başarıyla getirildi</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<Property>> GetAll()
        {
            try
            {
                var list = new List<Property>
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

                _logger.LogInformation("Toplam {Count} ilan getirildi.", list.Count);
                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "İlanlar getirilirken hata oluştu.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Veritabanı hatası oluştu.");
            }
        }
    }
}