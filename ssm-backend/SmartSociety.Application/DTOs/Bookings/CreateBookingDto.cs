namespace SmartSociety.Application.DTOs;

public class CreateBookingDto
{
    public Guid FacilityId {get; set;}
    public DateTime Date {get; set;}
    public DateTime StartTime {get; set;} 
    public DateTime EndTime {get; set;}
}