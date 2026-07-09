using System.ComponentModel.DataAnnotations;

namespace SmartSociety.Application.DTOs;

public class ResetPasswordDto
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email {get; set;} = "";

    [Required]
    public string Token {get; set;} = "";

    [Required]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    [MaxLength(128)]
    public string NewPassword {get; set;} = "";
}
