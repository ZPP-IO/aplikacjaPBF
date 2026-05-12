using System.Text;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ProjectPBF.Services
{
    public class DevEmailSender : IEmailSender
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<DevEmailSender> _logger;

        public DevEmailSender(IWebHostEnvironment environment, ILogger<DevEmailSender> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var mailsFolder = Path.Combine(_environment.ContentRootPath, "DevMails");
            Directory.CreateDirectory(mailsFolder);

            var safeEmail = string.Join("_", email.Split(Path.GetInvalidFileNameChars()));
            var fileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{safeEmail}.html";
            var filePath = Path.Combine(mailsFolder, fileName);

            var content = $"""
                <html>
                <head>
                    <meta charset="utf-8" />
                    <title>{subject}</title>
                </head>
                <body style="font-family: Arial, sans-serif;">
                    <h2>DEV EMAIL</h2>
                    <p><strong>Do:</strong> {email}</p>
                    <p><strong>Temat:</strong> {subject}</p>
                    <hr />
                    {htmlMessage}
                </body>
                </html>
                """;

            await File.WriteAllTextAsync(filePath, content, Encoding.UTF8);

            _logger.LogInformation("DEV EMAIL zapisany do pliku: {FilePath}", filePath);
        }
    }
}