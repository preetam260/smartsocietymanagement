namespace SmartSociety.Application.DTOs;

public class VisitorEntryResponseDto
{
    public Guid Id {get; set;}
    public Guid VisitorId {get; set;}
    public string VisitorName {get; set;} = string.Empty;
    public DateTime CheckinTime {get; set;}
    public DateTime? CheckoutTime {get; set;}
    public Guid StaffId {get; set;}
    public string StaffName {get; set;} = string.Empty;
}