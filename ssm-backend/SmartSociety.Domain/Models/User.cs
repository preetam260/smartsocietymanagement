using SmartSociety.Domain.Enums;

namespace SmartSociety.Domain.Models;

public class User : BaseEntity
{
    public string Name {get; set;} = "";
    public string Email {get; set;} = "";
    public string PasswordHash {get; set;} = "";
    public string PhoneNumber {get; set;} = "";
    public bool IsActive {get; set;} = false;
    public UserRole Role {get; set;}
    public string? PasswordResetToken {get; set;}
    public DateTime? PasswordResetTokenExpiry {get; set;}
    public DateTime? LastLoginAt {get; set;}  
}
