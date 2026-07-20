using SmartSociety.Domain.Enums;

namespace SmartSociety.Domain.Models;

public class Complaint : BaseEntity
{
    public Guid UserId {get; set;}
    public Guid ApartmentId {get; set;}
    public string Title {get; set;} = string.Empty;
    public string Description {get; set;} = string.Empty;
    public ComplaintStatus Status {get; set;} = ComplaintStatus.Open;
    public string? AdminResponse {get; set;}

    public ComplaintCategory? Category {get; set;}
    public ComplaintPriority? Priority {get; set;}
    public string? DraftAdminResponse {get; set;}
    public string? PossibleDuplicateIdsCsv {get; set;}
    public bool TriageProcessed {get; set;} = false;

    public User? User {get; set;}
    public Apartment? Apartment {get; set;}
}
