using System.ComponentModel.DataAnnotations;
using SmartSociety.Domain.Enums;

namespace SmartSociety.Application.DTOs;

public class CreateUserDto
{
    [Required]
    [MaxLength(100)]
    public string Name {get; set;} = "";

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email {get; set;} = "";

    [Required]
    [MaxLength(20)]
    public string PhoneNumber {get; set;} = "";

    [Required]
    [EnumDataType(typeof(UserRole))]
    public UserRole Role {get; set;}
}