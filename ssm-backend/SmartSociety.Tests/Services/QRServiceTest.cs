using Moq;
using NUnit.Framework;
using Microsoft.AspNetCore.Http;
using SmartSociety.Application.Exceptions;
using SmartSociety.Application.Services;
using System.Text;

namespace SmartSociety.Tests.Services;

[TestFixture]
public class QRServiceTest
{
    private QRService _service;

    [SetUp]
    public void Setup()
    {
        _service = new QRService();
    }

    [Test]
    public async Task GenerateTokenAsync_ReturnsValidString()
    {
        var token = await _service.GenerateTokenAsync();
        Assert.That(token, Is.Not.Null);
        Assert.That(Guid.TryParse(token, out _), Is.True);
    }

    [Test]
    public async Task GenerateImageAsync_ReturnsByteGraphic()
    {
        var token = "test_qr_token";
        var imageBytes = await _service.GenerateImageAsync(token);
        
        Assert.That(imageBytes, Is.Not.Null);
        Assert.That(imageBytes.Length, Is.GreaterThan(0));
    }

    [Test]
    public void DecodeImageAsync_ThrowsBadRequest_WhenImageInvalid()
    {
        // 1. Completely empty stream
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        Assert.ThrowsAsync<BadRequestException>(async () => await _service.DecodeImageAsync(fileMock.Object));

        // 2. Corrupt or unreadable image bytes
        var corruptBytes = Encoding.UTF8.GetBytes("clearly_not_an_image");
        var fileMock2 = new Mock<IFormFile>();
        fileMock2.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(corruptBytes));

        Assert.ThrowsAsync<BadRequestException>(async () => await _service.DecodeImageAsync(fileMock2.Object));
    }

    [Test]
    public async Task GenerateAndDecode_EndToEnd()
    {
        // Generate a real QR code image
        var originalToken = "SmartSociety_Secret_Token_123";
        var imageBytes = await _service.GenerateImageAsync(originalToken);

        // Put it in a Mock IFormFile
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(imageBytes));

        // Decode the image bytes back to string
        var decodedToken = await _service.DecodeImageAsync(fileMock.Object);

        // Verify they match
        Assert.That(decodedToken, Is.EqualTo(originalToken));
    }
}
