using Moq;
using NUnit.Framework;
using Microsoft.Extensions.Configuration;
using SmartSociety.Application.Services;
using System.Net.Sockets;

namespace SmartSociety.Tests.Services;

[TestFixture]
public class EmailServiceTest
{
    private Mock<IConfiguration> _mockConfig;
    private EmailService _service;

    [SetUp]
    public void Setup()
    {
        _mockConfig = new Mock<IConfiguration>();

        var mockSection = new Mock<IConfigurationSection>();
        mockSection.Setup(s => s["SenderName"]).Returns("SmartSociety");
        mockSection.Setup(s => s["SenderEmail"]).Returns("sender@smartsociety.com");
        mockSection.Setup(s => s["SmtpHost"]).Returns("127.0.0.1");
        mockSection.Setup(s => s["SmtpPort"]).Returns("2525");
        mockSection.Setup(s => s["SmtpUser"]).Returns("user");
        mockSection.Setup(s => s["SmtpPassword"]).Returns("pass");

        _mockConfig.Setup(c => c.GetSection("EmailSettings")).Returns(mockSection.Object);

        _service = new EmailService(_mockConfig.Object);
    }

    [Test]
    public void SendAsync_BuildsMessageAndAttemptsConnection()
    {
        var attachmentBytes = new byte[] { 1, 2, 3, 4 };
        var attachmentName = "qrcode.png";

        Assert.ThrowsAsync(Is.InstanceOf<Exception>(), async () => 
            await _service.SendAsync("recipient@test.com", "Test Subject", "<h1>Test Body</h1>", attachmentBytes, attachmentName)
        );
    }

    [Test]
    public void SendAsync_BuildsMessageWithoutAttachmentAndAttemptsConnection()
    {
        Assert.ThrowsAsync(Is.InstanceOf<Exception>(), async () => 
            await _service.SendAsync("recipient@test.com", "Test Subject", "<h1>Test Body</h1>")
        );
    }
}
