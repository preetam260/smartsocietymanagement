using SmartSociety.Domain.Enums;

namespace SmartSociety.Domain.Models;

public class Resident : BaseEntity
{
    public Guid UserId {get; set;} // FK
    public Guid ApartmentId {get; set;} // FK
    public DateTime MoveInDate {get; set;}
    public DateTime? MoveOutDate {get; set;}
    public string? VehicleNumber {get; set;}

    public User? User {get; set;}
    public Apartment? Apartment {get; set;}
}