namespace SmartSociety.Application.DTOs;

public class BookingCalendarResponseDto
{
    public Guid FacilityId { get; set; }
    public string FacilityName { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public IEnumerable<BookingAvailabilitySlotDto> Slots { get; set; } = [];
}

public class BookingAvailabilitySlotDto
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int ConfirmedSeats { get; set; }
    public int HeldSeats { get; set; }
    public int ReservedSeats { get; set; }
    public int AvailableSeats { get; set; }
    public string AvailabilityLevel { get; set; } = string.Empty;
}
