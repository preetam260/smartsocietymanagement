namespace SmartSociety.Application.DTOs;

public class CreateComplaintDto
{
    public Guid ApartmentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
