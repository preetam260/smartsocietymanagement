using SmartSociety.Domain.Enums;

namespace SmartSociety.Application.DTOs;
public class AnnouncementResponseDto
{
    public Guid Id {get; set;}
    public string Title {get; set;} = string.Empty;
    public string Content {get; set;} = string.Empty;
    public UserRole Audience {get; set;}
    public bool IsPinned {get; set;}
    public DateTime ExpiresAt {get; set;}
    public DateTime CreatedAt {get; set;}
}