using Microsoft.AspNetCore.Mvc;
using RealEstate.Api.Entities;
using RealEstate.Api.Data;
using RealEstate.Api.Dtos;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace RealEstate.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImagesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ImagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/Images
        [HttpPost]
        public async Task<IActionResult> AddImage([FromBody] ImageCreateDto dto)
        {
            // İlan var mı kontrolü
            var listing = await _context.Listings.FindAsync(dto.ListingId);
            if (listing == null)
                return NotFound("Listing not found");

            var image = new Image
            {
                ListingId = dto.ListingId,
                Base64Data = dto.Base64Data,
                FileName = dto.FileName
            };
            _context.Images.Add(image);
            await _context.SaveChangesAsync();

            var result = new ImageDto
            {
                Id = image.Id,
                ListingId = image.ListingId,
                Base64Data = image.Base64Data,
                FileName = image.FileName
            };
            return Ok(result);
        }

        // GET: api/Images/listing/1
        [HttpGet("listing/{listingId}")]
        public async Task<IActionResult> GetImagesByListing(int listingId)
        {
            var images = await _context.Images
                .Where(i => i.ListingId == listingId)
                .Select(i => new ImageDto
                {
                    Id = i.Id,
                    ListingId = i.ListingId,
                    Base64Data = i.Base64Data,
                    FileName = i.FileName
                })
                .ToListAsync();
            return Ok(images);
        }
    }
}
