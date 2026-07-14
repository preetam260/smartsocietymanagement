using SmartSociety.Domain.Enums;

namespace SmartSociety.Domain.Models;

public class Resident : BaseEntity
{
    public Guid UserId {get; set;}
    public Guid ApartmentId {get; set;} 
    public DateTime MoveInDate {get; set;}
    public DateTime? MoveOutDate {get; set;}
    public string? VehicleNumber {get; set;}

    public User? User {get; set;}
    public Apartment? Apartment {get; set;}
}