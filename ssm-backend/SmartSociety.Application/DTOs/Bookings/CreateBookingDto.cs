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

    [Range(1, 500, ErrorMessage = "Seats booked must be at least 1.")]
    public int SeatsBooked {get; set;} = 1;

    /// <summary>
    /// When true, ignores SeatsBooked and books the entire facility capacity.
    /// Useful for community hall, party hall, or exclusive event bookings.
    /// </summary>
    public bool BookFullFacility {get; set;} = false;
}
