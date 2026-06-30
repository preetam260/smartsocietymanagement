using SmartSociety.Domain.Enums;

namespace SmartSociety.Application.DTOs;

public class CreateUserDto
{
    public string Name {get; set;} = "";
    public string Email {get; set;} = "";
    // public string Password {get; set;} = ""; test instead of admin actually creating the password for the user well employ a random password generator
    public string PhoneNumber {get; set;} = "";
    public UserRole Role {get; set;}
}