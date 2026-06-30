namespace SmartSociety.Application.DTOs;

public class FacilityDto
{
    public string Name {get; set;} = "";
    public string Description {get; set;} = "";
    public decimal HourlyRate  {get; set;}
    public int Capacity {get; set;}
    public bool IsActive {get; set;}
}