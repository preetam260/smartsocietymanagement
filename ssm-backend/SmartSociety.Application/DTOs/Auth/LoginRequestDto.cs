using System.ComponentModel.DataAnnotations;

namespace SmartSociety.Application.DTOs;

public class LoginRequestDto
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email {get; set;} = "";

    [Required]
    [MinLength(6)]
    public string Password {get; set;} = "";
}