namespace SmartSociety.Application.DTOs;

public class ApartmentResponseDto
{
    public Guid Id {get; set;}
    public string Block {get; set;} = "";
    public int Floor {get; set;}
    public string Number {get; set;} = "";
    public Guid OwnerId {get; set;}
    public string OwnerName {get; set;} = "";
}