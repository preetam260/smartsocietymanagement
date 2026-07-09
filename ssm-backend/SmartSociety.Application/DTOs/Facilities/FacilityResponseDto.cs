namespace SmartSociety.Application.DTOs;

public class FacilityResponseDto
{
    public Guid Id {get; set;}
    public string Name {get; set;} = "";
    public string Description {get; set;} = "";
    public decimal HourlyRate {get; set;}
    public int Capacity {get; set;}
    public bool IsActive  {get; set;}

}