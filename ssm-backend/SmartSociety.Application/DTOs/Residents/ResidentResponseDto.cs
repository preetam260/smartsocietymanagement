namespace SmartSociety.Application.DTOs;

public class ResidentResponseDto
{
    public Guid Id  {get; set;}
    public Guid UserId {get; set;}
    public string UserName {get; set;} = string.Empty;
    public string UserEmail {get; set;} = string.Empty;
    public Guid ApartmentId {get; set;}
    public string ApartmentBlock {get; set;} = string.Empty;
    public string ApartmentNumber {get; set;} = string.Empty;
    public DateTime MoveInDate {get; set;}
    public DateTime? MoveOutDate {get; set;}
    public string? VehicleNumber {get; set;}
}