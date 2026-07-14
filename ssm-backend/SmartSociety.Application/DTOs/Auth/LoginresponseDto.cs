using SmartSociety.Domain.Enums;

namespace SmartSociety.Application.DTOs;

public class LoginResponseDto
{
    public string Token {get; set;} = string.Empty;
    public string Name {get; set;} = string.Empty;
    public string Email {get; set;} = string.Empty;
    public UserRole Role {get; set;}
    public bool HasActiveResidency {get; set;}

}