using MailKit.Net.Smtp;
using MailKit.Security;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using MimeKit;

public class EmailService : IEmailService
{
    private readonly IConfiguration _cfg;
    public EmailService(IConfiguration cfg) => _cfg = cfg;

    public async Task SendAsync(string to, string subject, string htmlBody)
    {
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
    }
}