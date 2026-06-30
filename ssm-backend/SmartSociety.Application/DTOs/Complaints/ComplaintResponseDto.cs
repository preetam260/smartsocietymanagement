using SmartSociety.Domain.Enums;

namespace SmartSociety.Application.DTOs;

public class ComplaintResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public Guid ApartmentId { get; set; }
    public string ApartmentBlock { get; set; } = string.Empty;
    public string ApartmentNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ComplaintStatus Status { get; set; }
    public string? AdminResponse { get; set; }
    public DateTime CreatedAt { get; set; }
}
