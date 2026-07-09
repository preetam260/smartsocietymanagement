namespace SmartSociety.Application.Interfaces;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string body, byte[]? attachmentBytes = null, string? attachmentName = null);
}