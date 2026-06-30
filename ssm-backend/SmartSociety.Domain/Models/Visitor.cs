using SmartSociety.Domain.Enums;

namespace SmartSociety.Domain.Models;

public class Visitor : BaseEntity
{
    public string Name {get; set;} = "";
    public string Email {get; set;} = "";
    public string Purpose {get; set;} = "";
    public Guid ApartmentId {get; set;} 
    public string QrToken {get; set;} = "";
    public DateTime ETA {get; set;} 
    public DateTime ExpiresAt {get; set;}
    public VisitorStatus Status {get; set;} = VisitorStatus.Pending;

    public Apartment? Apartment {get; set;}
    public ICollection<VisitorEntry> Entries {get; set;} = new List<VisitorEntry>();

}