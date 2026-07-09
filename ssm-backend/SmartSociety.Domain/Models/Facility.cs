namespace SmartSociety.Domain.Models;

public class Facility : BaseEntity
{
    public string Name {get; set;} = string.Empty;
    public string Description {get; set;} = string.Empty;
    public decimal HourlyRate {get; set;}
    public int Capacity {get; set;}
    public bool IsActive {get; set;} = true;

    public ICollection<Booking> Bookings {get; set;} = new List<Booking>();
}