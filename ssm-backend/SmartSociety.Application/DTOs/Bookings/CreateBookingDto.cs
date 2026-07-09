using System.ComponentModel.DataAnnotations;

namespace SmartSociety.Application.DTOs;

public class CreateBookingDto
{
    [Required]
    public Guid FacilityId {get; set;}

    [Required]
    public DateTime Date {get; set;}

    [Required]
    public DateTime StartTime {get; set;}

    [Required]
    public DateTime EndTime {get; set;}
}