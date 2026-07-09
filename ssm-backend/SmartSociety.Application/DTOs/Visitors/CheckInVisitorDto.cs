using Microsoft.AspNetCore.Http;

namespace SmartSociety.Application.DTOs;

public class CheckInVisitorDto
{
    public IFormFile? QrToken {get; set;}
}