using System.ComponentModel.DataAnnotations;

namespace SmartSociety.Application.DTOs;

public class UpdateUserDto
{
    [Required]
    [MaxLength(100)]
    public string Name {get; set;} = "";

    [Required]
    [MaxLength(20)]
    public string PhoneNumber {get; set;} = "";
}