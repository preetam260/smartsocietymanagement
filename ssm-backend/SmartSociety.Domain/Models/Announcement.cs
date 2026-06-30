using SmartSociety.Domain.Enums;

namespace SmartSociety.Domain.Models;

public class Announcement : BaseEntity
{
    public string Title {get; set;} = string.Empty;
    public string Content {get; set;} = string.Empty;
    public UserRole Audience {get; set;}
    public bool IsPinned {get; set;} = false;
    public DateTime ExpiresAt {get; set;}
}