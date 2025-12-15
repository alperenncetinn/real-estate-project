using Microsoft.AspNetCore.Mvc;
using RealEstate.Api.Dtos;

namespace RealEstate.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ListingsController : ControllerBase
    {
        [HttpPost]
        public IActionResult Create([FromForm] ListingDto dto)
        {
            // Debugging: Check if data arrived
            if (dto == null) return BadRequest("No data received");

            // TODO: Save to Database (Next Step)
            
            // For now, return Success to confirm connection
            return Ok(new { message = "Listing received successfully", title = dto.Title });
        }
    }
}