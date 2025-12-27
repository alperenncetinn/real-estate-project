using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RealEstate.Api.Services
{
    public interface IEmailService
    {
        Task<bool> SendVerificationCodeAsync(string toEmail, string code);
    }

    public class EmailService : IEmailService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(HttpClient httpClient, IConfiguration configuration, ILogger<EmailService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendVerificationCodeAsync(string toEmail, string code)
        {
            try
            {
                var apiKey = _configuration["Email:ApiKey"];
                var senderEmail = _configuration["Email:SenderEmail"] ?? "onboarding@resend.dev";
                var senderName = _configuration["Email:SenderName"] ?? "Senin Evin";

                // Eğer senderEmail onboarding@resend.dev ise ve alıcı biz değilsek, Resend hata verebilir (test modunda).
                // Ancak kullanıcı kendi mailine test edeceği için sorun olmamalı.
                _logger.LogInformation("Sending email via Resend API to {Email} from {Sender}", toEmail, senderEmail);

                var emailData = new
                {
                    from = $"{senderName} <{senderEmail}>",
                    to = new[] { toEmail },
                    subject = "🏠 Senin Evin - E-posta Doğrulama Kodu",
                    html = $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <style>
                                body {{ font-family: 'Segoe UI', Arial, sans-serif; background-color: #fdf2f8; padding: 40px 20px; }}
                                .container {{ max-width: 500px; margin: 0 auto; background: white; border-radius: 20px; padding: 40px; box-shadow: 0 10px 40px rgba(236,72,153,0.1); }}
                                .logo {{ text-align: center; margin-bottom: 30px; }}
                                .logo span {{ font-size: 32px; }}
                                h1 {{ color: #ec4899; text-align: center; margin-bottom: 10px; font-size: 24px; }}
                                .subtitle {{ text-align: center; color: #6b7280; margin-bottom: 30px; }}
                                .code-box {{ background: linear-gradient(135deg, #f472b6, #ec4899); color: white; font-size: 36px; font-weight: bold; text-align: center; padding: 20px 40px; border-radius: 15px; letter-spacing: 8px; margin: 30px 0; }}
                                .info {{ color: #6b7280; font-size: 14px; text-align: center; margin-top: 30px; }}
                                .warning {{ background: #fef3c7; color: #92400e; padding: 15px; border-radius: 10px; font-size: 13px; margin-top: 20px; }}
                                .footer {{ text-align: center; margin-top: 30px; color: #9ca3af; font-size: 12px; }}
                            </style>
                        </head>
                        <body>
                            <div class='container'>
                                <div class='logo'><span>🏠</span></div>
                                <h1>E-posta Doğrulama</h1>
                                <p class='subtitle'>Senin Evin hesabını doğrulamak için aşağıdaki kodu kullan</p>
                                
                                <div class='code-box'>{code}</div>
                                
                                <p class='info'>Bu kod <strong>10 dakika</strong> içinde geçerliliğini yitirecektir.</p>
                                
                                <div class='warning'>
                                    ⚠️ Bu kodu kimseyle paylaşmayın. Senin Evin ekibi sizden asla doğrulama kodu istemez.
                                </div>
                                
                                <p class='footer'>
                                    💕 Senin Evin - Hayalindeki evi bul!<br>
                                    Bu e-postayı siz talep etmediyseniz, lütfen dikkate almayın.
                                </p>
                            </div>
                        </body>
                        </html>
                    "
                };

                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                request.Content = new StringContent(JsonSerializer.Serialize(emailData), Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Email sent successfully. Response: {Response}", responseContent);
                    return true;
                }
                else
                {
                    _logger.LogError("Failed to send email. Status: {Status}, Response: {Response}", response.StatusCode, responseContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while sending email via Resend API");
                return false;
            }
        }
    }
}
