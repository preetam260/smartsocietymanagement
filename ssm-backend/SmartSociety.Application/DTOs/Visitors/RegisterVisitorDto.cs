using System.ComponentModel.DataAnnotations;

namespace SmartSociety.Application.DTOs;

public class RegisterVisitorDto
{
    [Required]
    [MaxLength(100)]
    public string Name {get; set;} = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email {get; set;} = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Purpose {get; set;} = string.Empty;

    [Required]
    public DateTime ETA {get; set;}
}