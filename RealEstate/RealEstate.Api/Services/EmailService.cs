using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace RealEstate.Api.Services
{
    public interface IEmailService
    {
        Task<bool> SendVerificationCodeAsync(string toEmail, string code);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendVerificationCodeAsync(string toEmail, string code)
        {
            try
            {
                var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var senderEmail = _configuration["Email:SenderEmail"] ?? "seninevinauth@gmail.com";
                var senderPassword = _configuration["Email:SenderPassword"] ?? "uzfs pvzl uunk hoxl";
                var senderName = _configuration["Email:SenderName"] ?? "Senin Evin";

                // Log without password
                _logger.LogInformation("Attempting to connect to SMTP: Host={Host}, Port={Port}, User={User}", 
                    smtpHost, smtpPort, senderEmail);

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(senderName, senderEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = "🏠 Senin Evin - E-posta Doğrulama Kodu";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
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

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                // Accept all certificates (for debugging) - Production'da kaldırılabilir ama 587 için güvenli
                client.CheckCertificateRevocation = false;

                // 20 sn timeout
                client.Timeout = 20000;

                await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
                
                await client.AuthenticateAsync(senderEmail, senderPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                
                _logger.LogInformation("Verification email sent successfully to {Email}", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send verification email. Error: {Error}", ex.Message);
                return false;
            }
        }
    }
}
