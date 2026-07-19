using SmartSociety.Domain.Enums;

namespace SmartSociety.Application.DTOs;

public class VisitorHistoryDto
{
    public string VisitorName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int TotalVisits { get; set; }
    public List<VisitorPassHistoryDto> Passes { get; set; } = new();
}

public class VisitorPassHistoryDto
{
    public Guid VisitorId { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public DateTime ETA { get; set; }
    public VisitorStatus Status { get; set; }
    public List<VisitorEntryResponseDto> Entries { get; set; } = new();
}