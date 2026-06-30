namespace SmartSociety.Application.DTOs;

public class CreateBillDto
{
    public Guid ApartmentId {get; set;}
    public string Period {get; set;} = string.Empty; // "MM-YYYY"
    public decimal BaseAmount {get; set;}
    public DateTime DueDate {get; set;}
}
