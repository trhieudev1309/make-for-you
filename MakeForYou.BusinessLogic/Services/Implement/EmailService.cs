using MailKit.Net.Smtp;
using MailKit.Security;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

public class EmailService : IEmailService
{
    private readonly IConfiguration _cfg;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration cfg, ILogger<EmailService> logger)
    {
        _cfg = cfg;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string htmlBody)
    {
        _logger.LogInformation("Sending email: to={To}, subject={Subject}", to, subject);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Make For You", _cfg["Email:From"]));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();

        // Kết nối dùng StartTls (cho cổng 587)
        await client.ConnectAsync(_cfg["Email:Host"], int.Parse(_cfg["Email:Port"]!), SecureSocketOptions.StartTls);

        // Đăng nhập
        await client.AuthenticateAsync(_cfg["Email:User"], _cfg["Email:Pass"]);

        await client.SendAsync(message);
        await client.DisconnectAsync(true);

        _logger.LogInformation("Email sent successfully: to={To}, subject={Subject}", to, subject);
    }
}