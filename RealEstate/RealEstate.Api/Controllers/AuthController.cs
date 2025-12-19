using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RealEstate.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequestDto request)
        {
            if (request == null)
                return BadRequest(new { message = "Request body cannot be null." });

            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Email and password are required." });

            // TEMPORARY MOCK LOGIN
            if (request.Email == "test@test.com" && request.Password == "123456")
            {
                return Ok(new
                {
                    token = "fake-jwt-token",
                    message = "Login successful."
                });
            }

            return Unauthorized(new { message = "Invalid email or password." });
        }

    }
}
