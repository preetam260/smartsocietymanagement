using System.ComponentModel.DataAnnotations;

namespace SmartSociety.Application.DTOs;

public class CreateResidentDto
{
    [Required]
    public Guid UserId {get; set;}

    [Required]
    public Guid ApartmentId {get; set;}

    [Required]
    public DateTime MoveInDate {get; set;}

    [MaxLength(20)]
    public string? VehicleNumber {get; set;}
}