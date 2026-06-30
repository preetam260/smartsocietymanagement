using SmartSociety.Domain.Enums;

namespace SmartSociety.Application.DTOs;

public class UserResponseDto
{
    public Guid Id {get; set;}
    public string Name {get; set;} = "";
    public string Email {get; set;} = "";
    public string PhoneNumber {get; set;} = "";
    public UserRole Role {get; set;}
    public bool IsActive {get; set;}
}