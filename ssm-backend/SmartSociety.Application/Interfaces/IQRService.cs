using Microsoft.AspNetCore.Http;

namespace SmartSociety.Application.Interfaces;

public interface IQRService
{
    Task<string> GenerateTokenAsync();
    Task<byte[]> GenerateImageAsync(string token);
    Task<string> DecodeImageAsync(IFormFile image);
}