using System.ComponentModel.DataAnnotations;

namespace SmartSociety.Application.DTOs;

public class ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email {get; set;} = "";
}
