using System.Net.Http.Json;
using System.Text.Json;
using RealEstate.Web.Models;

namespace RealEstate.Web.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthService> _logger;

        public AuthService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<AuthService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<(bool Success, string? ErrorMessage, LoginResponseDto? Data)> LoginAsync(string email, string password)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", new { email, password });

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
                    return (true, null, data);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                try
                {
                    var errorObj = JsonSerializer.Deserialize<JsonElement>(errorContent);
                    if (errorObj.TryGetProperty("message", out var messageElement))
                    {
                        return (false, messageElement.GetString(), null);
                    }
                }
                catch { }

                return (false, "Giriş başarısız. Lütfen bilgilerinizi kontrol edin.", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error");
                return (false, "Bir hata oluştu. Lütfen daha sonra tekrar deneyin.", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, LoginResponseDto? Data)> RegisterAsync(RegisterViewModel model)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/register", new
                {
                    email = model.Email,
                    password = model.Password,
                    firstName = model.FirstName,
                    lastName = model.LastName,
                    phoneNumber = model.PhoneNumber
                });

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
                    return (true, null, data);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                try
                {
                    var errorObj = JsonSerializer.Deserialize<JsonElement>(errorContent);
                    if (errorObj.TryGetProperty("message", out var messageElement))
                    {
                        return (false, messageElement.GetString(), null);
                    }
                }
                catch { }

                return (false, "Kayıt başarısız. Lütfen bilgilerinizi kontrol edin.", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Register error");
                return (false, "Bir hata oluştu. Lütfen daha sonra tekrar deneyin.", null);
            }
        }

        public void SaveUserSession(LoginResponseDto loginData, bool rememberMe)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return;

            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine($"[AUTH] SaveUserSession çağrıldı");
            Console.WriteLine($"[AUTH] Token: {(string.IsNullOrEmpty(loginData.Token) ? "BOŞ!!!" : loginData.Token.Substring(0, Math.Min(30, loginData.Token.Length)) + "...")}");
            Console.WriteLine($"[AUTH] UserId: {loginData.Id}");
            Console.WriteLine($"[AUTH] Email: {loginData.Email}");
            Console.WriteLine("--------------------------------------------------");

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // Development için false, Production'da true olmalı
                SameSite = SameSiteMode.Lax,
                Expires = rememberMe ? DateTimeOffset.UtcNow.AddDays(7) : null
            };

            context.Response.Cookies.Append("auth_token", loginData.Token, cookieOptions);
            context.Response.Cookies.Append("user_id", loginData.Id.ToString(), cookieOptions);
            context.Response.Cookies.Append("user_email", loginData.Email, cookieOptions);
            context.Response.Cookies.Append("user_name", $"{loginData.FirstName} {loginData.LastName}", cookieOptions);
            context.Response.Cookies.Append("user_role", loginData.Role, cookieOptions);
        }


        public void ClearUserSession()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return;

            context.Response.Cookies.Delete("auth_token");
            context.Response.Cookies.Delete("user_id");
            context.Response.Cookies.Delete("user_email");
            context.Response.Cookies.Delete("user_name");
            context.Response.Cookies.Delete("user_role");
        }

        public UserInfoViewModel? GetCurrentUser()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            var token = context.Request.Cookies["auth_token"];
            if (string.IsNullOrEmpty(token)) return null;

            var userIdStr = context.Request.Cookies["user_id"];
            var email = context.Request.Cookies["user_email"];
            var name = context.Request.Cookies["user_name"];
            var role = context.Request.Cookies["user_role"];

            if (string.IsNullOrEmpty(email)) return null;

            var nameParts = (name ?? "").Split(' ', 2);
            int.TryParse(userIdStr, out int userId);

            return new UserInfoViewModel
            {
                Id = userId,
                Email = email,
                FirstName = nameParts.Length > 0 ? nameParts[0] : "",
                LastName = nameParts.Length > 1 ? nameParts[1] : "",
                Role = role ?? "User"
            };
        }

        public bool IsAuthenticated()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return false;

            var token = context.Request.Cookies["auth_token"];
            return !string.IsNullOrEmpty(token);
        }

        public bool IsAdmin()
        {
            var user = GetCurrentUser();
            return user?.Role == "Admin";
        }

        public string? GetToken()
        {
            var context = _httpContextAccessor.HttpContext;
            return context?.Request.Cookies["auth_token"];
        }

        public async Task<ProfileViewModel?> GetProfileAsync()
        {
            try
            {
                var token = GetToken();
                if (string.IsNullOrEmpty(token)) return null;

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync("api/auth/me");
                if (response.IsSuccessStatusCode)
                {
                    var user = await response.Content.ReadFromJsonAsync<ProfileViewModel>();
                    return user;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetProfile error");
                return null;
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateProfileAsync(ProfileViewModel model)
        {
            try
            {
                var token = GetToken();
                if (string.IsNullOrEmpty(token))
                    return (false, "Oturum açmanız gerekiyor.");

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PutAsJsonAsync("api/auth/profile", new
                {
                    firstName = model.FirstName,
                    lastName = model.LastName,
                    phoneNumber = model.PhoneNumber
                });

                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                try
                {
                    var errorObj = JsonSerializer.Deserialize<JsonElement>(errorContent);
                    if (errorObj.TryGetProperty("message", out var messageElement))
                    {
                        return (false, messageElement.GetString());
                    }
                }
                catch { }

                return (false, "Profil güncellenemedi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateProfile error");
                return (false, "Bir hata oluştu. Lütfen daha sonra tekrar deneyin.");
            }
        }
    }
}
