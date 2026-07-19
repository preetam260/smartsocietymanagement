using SmartSociety.Domain.Enums;

namespace SmartSociety.Application.DTOs;

public class VisitorResponseDto
{
    public Guid Id {get; set;}
    public string Name {get; set;} = string.Empty;
    public string Email {get; set;} = string.Empty;
    public string Purpose {get; set;} = string.Empty;
    public Guid ApartmentId {get; set;}
    public string ApartmentBlock {get; set;} = string.Empty;
    public string ApartmentNumber {get; set;} = string.Empty;
    public string QrToken {get; set;} = string.Empty;
    public DateTime ETA {get; set;}
    public DateTime ExpiresAt {get; set;}
    public VisitorStatus Status {get; set;}
    public DateTime? UpdatedAt { get; set; }
}