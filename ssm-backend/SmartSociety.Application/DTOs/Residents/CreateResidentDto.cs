namespace SmartSociety.Application.DTOs;

public class CreateResidentDto
{
    public Guid UserId {get; set;}
    public Guid ApartmentId {get; set;}
    public DateTime MoveInDate {get; set;}
    public string? VehicleNumber {get; set;}
}