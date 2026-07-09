using System.ComponentModel.DataAnnotations;
using SmartSociety.Domain.Enums;

namespace SmartSociety.Application.DTOs;

public class AnnouncementDto
{
    [Required]
    [MaxLength(200)]
    public string Title {get; set;} = string.Empty;

    [Required]
    [MaxLength(5000)]
    public string Content {get; set;} = string.Empty;

    [Required]
    [EnumDataType(typeof(UserRole))]
    public UserRole Audience {get; set;}

    public bool IsPinned {get; set;}

    [Required]
    public DateTime ExpiresAt {get; set;}
}