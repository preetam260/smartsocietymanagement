namespace SmartSociety.Application.DTOs;

public class CreateApartmentDto {
    public Guid OwnerId {get; set;}
    public string Block {get; set;} = "";
    public int Floor {get; set;}
    public string Number {get; set;} = "";
}