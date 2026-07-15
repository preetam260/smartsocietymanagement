using SmartSociety.Domain.Enums;

namespace SmartSociety.Application.DTOs;

public class BookingResponseDto{
    public Guid Id {get; set;} 
    public Guid FacilityId {get; set;}
    public string FacilityName {get; set;} = string.Empty;
    public Guid UserId {get; set;}
    public string UserName {get; set;} = string.Empty;
    public DateTime Date {get; set;}
    public DateTime StartTime {get; set;}
    public DateTime EndTime {get; set;}
    public int SeatsBooked {get; set;}
    public decimal TotalCost {get; set;}
    public BookingStatus Status {get; set;}
    public DateTime? HoldExpiresAt {get; set;}
    public string? TransactionRef {get; set;}
}
