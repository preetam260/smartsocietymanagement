using SmartSociety.Domain.Enums;

namespace SmartSociety.Application.DTOs;

public class ComplaintTriageResult
{
    public ComplaintCategory Category { get; set; }
    public ComplaintPriority Priority { get; set; }
    public string DraftResponse { get; set; } = string.Empty;
    public List<Guid> PossibleDuplicateIds { get; set; } = new();
}