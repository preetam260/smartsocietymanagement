using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using SmartSociety.Application.Interfaces;

namespace SmartSociety.Application.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendAsync(string to, string subject, string body, byte[]? attachmentBytes = null, string? attachmentName = null)
    {
        var settings = _config.GetSection("EmailSettings");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(settings["SenderName"], settings["SenderEmail"]));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        var builder = new BodyBuilder();
        builder.HtmlBody = body;

        if (attachmentBytes != null && attachmentBytes.Length > 0 && !string.IsNullOrEmpty(attachmentName))
        {
            var image = builder.LinkedResources.Add(attachmentName, attachmentBytes);
            image.ContentId = "qrcode";
        }

        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(
            settings["SmtpHost"],
            int.Parse(settings["SmtpPort"]!),
            MailKit.Security.SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(settings["SmtpUser"], settings["SmtpPassword"]);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}