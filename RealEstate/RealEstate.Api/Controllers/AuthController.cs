using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstate.Api.Data;
using RealEstate.Api.Dtos;
using RealEstate.Api.Entities;
using RealEstate.Api.Services;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RealEstate.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthController(ApplicationDbContext context, IJwtService jwtService, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _jwtService = jwtService;
            _configuration = configuration;
            _emailService = emailService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (request == null)
                return BadRequest(new { message = "Request body cannot be null." });

            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Email and password are required." });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid email or password." });

            if (!user.IsActive)
                return Unauthorized(new { message = "User account is deactivated." });

            var token = _jwtService.GenerateToken(user);
            var expirationMinutes = int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "60");

            return Ok(new LoginResponseDto
            {
                Id = user.Id,
                Token = token,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            if (request == null)
                return BadRequest(new { message = "Request body cannot be null." });

            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Email and password are required." });

            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest(new { message = "Email is already registered." });

            var user = new User
            {
                Email = request.Email,
                PasswordHash = HashPassword(request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                Role = "User",
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = _jwtService.GenerateToken(user);
            var expirationMinutes = int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "60");

            return Ok(new LoginResponseDto
            {
                Id = user.Id,
                Token = token,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            return Ok(new { message = "Logout successful." });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { message = "Invalid token." });

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found." });

            return Ok(new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                CreatedDate = user.CreatedDate,
                IsActive = user.IsActive
            });
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Request body cannot be null." });

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { message = "Invalid token." });

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found." });

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.PhoneNumber = dto.PhoneNumber;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Profil güncellendi." });
        }

        // ==================== ADMIN ENDPOINTS ====================

        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Role = u.Role,
                    CreatedDate = u.CreatedDate,
                    IsActive = u.IsActive
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPut("users/{id}/role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateUserRoleDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Role))
                return BadRequest(new { message = "Role is required." });

            if (dto.Role != "User" && dto.Role != "Admin")
                return BadRequest(new { message = "Invalid role. Allowed roles: User, Admin" });

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found." });

            user.Role = dto.Role;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"User role updated to {dto.Role}." });
        }

        [HttpPut("users/{id}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(currentUserIdClaim) && int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                if (currentUserId == id)
                    return BadRequest(new { message = "You cannot deactivate your own account." });
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found." });

            user.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok(new { message = "User has been deactivated." });
        }

        [HttpPut("users/{id}/activate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActivateUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found." });

            user.IsActive = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "User has been activated." });
        }

        [HttpDelete("users/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(currentUserIdClaim) && int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                if (currentUserId == id)
                    return BadRequest(new { message = "You cannot delete your own account." });
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found." });

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User has been deleted." });
        }

        // ==================== EMAIL VERIFICATION ====================

        [HttpPost("send-verification-code")]
        public async Task<IActionResult> SendVerificationCode([FromBody] SendVerificationCodeDto request)
        {
            if (string.IsNullOrWhiteSpace(request?.Email))
                return BadRequest(new { message = "Email adresi gerekli." });

            // Mevcut kullanıcı var mı kontrol et
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest(new { message = "Bu email adresi zaten kayıtlı." });

            // Eski kodları geçersiz yap
            var oldCodes = await _context.VerificationCodes
                .Where(v => v.Email == request.Email && !v.IsUsed && v.Type == "email_verification")
                .ToListAsync();
            
            foreach (var oldCode in oldCodes)
            {
                oldCode.IsUsed = true;
            }

            // Yeni kod oluştur (6 haneli)
            var code = new Random().Next(100000, 999999).ToString();

            var verificationCode = new VerificationCode
            {
                Email = request.Email,
                Code = code,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                Type = "email_verification"
            };

            _context.VerificationCodes.Add(verificationCode);
            await _context.SaveChangesAsync();

            // Email gönder
            var emailSent = await _emailService.SendVerificationCodeAsync(request.Email, code);

            if (!emailSent)
                return StatusCode(500, new { message = "Email gönderilemedi. Lütfen tekrar deneyin." });

            return Ok(new { message = "Doğrulama kodu email adresinize gönderildi." });
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto request)
        {
            if (string.IsNullOrWhiteSpace(request?.Email) || string.IsNullOrWhiteSpace(request?.Code))
                return BadRequest(new { message = "Email ve doğrulama kodu gerekli." });

            var verificationCode = await _context.VerificationCodes
                .Where(v => v.Email == request.Email 
                         && v.Code == request.Code 
                         && !v.IsUsed 
                         && v.Type == "email_verification"
                         && v.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(v => v.CreatedAt)
                .FirstOrDefaultAsync();

            if (verificationCode == null)
                return BadRequest(new { message = "Geçersiz veya süresi dolmuş doğrulama kodu." });

            // Kodu kullanıldı olarak işaretle
            verificationCode.IsUsed = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Email doğrulandı!", verified = true });
        }

        // ==================== HELPER METHODS ====================

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private static bool VerifyPassword(string password, string passwordHash)
        {
            var hash = HashPassword(password);
            return hash == passwordHash;
        }
    }
}
