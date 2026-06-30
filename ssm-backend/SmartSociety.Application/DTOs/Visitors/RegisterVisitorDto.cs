namespace SmartSociety.Application.DTOs;

public class RegisterVisitorDto
{
    public string Name {get; set;} = string.Empty;
    public string Email {get; set;} = string.Empty;
    public string Purpose {get; set;} = string.Empty;
    public DateTime ETA {get; set;}    
}