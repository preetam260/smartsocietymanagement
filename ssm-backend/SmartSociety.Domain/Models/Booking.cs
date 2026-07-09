using SmartSociety.Domain.Enums;

namespace SmartSociety.Domain.Models;

public class Booking : BaseEntity
{
    public Guid FacilityId {get; set;} 
    public Guid UserId {get; set;} 
    public DateTime Date {get; set;}
    public DateTime StartTime {get; set;}
    public DateTime EndTime {get; set;}
    public decimal TotalCost {get; set;}
    public BookingStatus Status {get; set;} = BookingStatus.Pending;
    public DateTime? HoldExpiresAt {get; set;}
    public string? TransactionRef {get; set;}

    public User? User {get; set;}
    public Facility? Facility {get; set;}
}