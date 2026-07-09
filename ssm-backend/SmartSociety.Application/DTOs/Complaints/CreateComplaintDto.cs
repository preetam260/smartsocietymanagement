using System.ComponentModel.DataAnnotations;

namespace SmartSociety.Application.DTOs;

public class CreateComplaintDto
{
    [Required]
    public Guid ApartmentId {get; set;}

    [Required]
    [MaxLength(200)]
    public string Title {get; set;} = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Description {get; set;} = string.Empty;
}
