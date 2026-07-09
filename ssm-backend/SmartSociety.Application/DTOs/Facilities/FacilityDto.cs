using System.ComponentModel.DataAnnotations;

namespace SmartSociety.Application.DTOs;

public class FacilityDto
{
    [Required]
    [MaxLength(100)]
    public string Name {get; set;} = "";

    [MaxLength(1000)]
    public string Description {get; set;} = "";

    [Required]
    [Range(0, 999999, ErrorMessage = "Hourly rate must be 0 or more.")]
    public decimal HourlyRate {get; set;}

    [Required]
    [Range(1, 10000, ErrorMessage = "Capacity must be at least 1.")]
    public int Capacity {get; set;}

    public bool IsActive {get; set;}
}