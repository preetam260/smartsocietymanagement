using System.ComponentModel.DataAnnotations;

namespace SmartSociety.Application.DTOs;

public class CreateApartmentDto
{
    [Required]
    public Guid OwnerId {get; set;}

    [Required]
    [MaxLength(1)]
    public string Block {get; set;} = "";

    [Range(0, 20, ErrorMessage = "Floor must be between 0 and 20.")]
    public int Floor {get; set;}

    [Required]
    [MaxLength(3)]
    public string Number {get; set;} = "";
}