using System.ComponentModel.DataAnnotations;

namespace SmartSociety.Application.DTOs;

public class CreateBillDto
{
    [Required]
    public Guid ApartmentId {get; set;}

    [Required]
    [RegularExpression(@"^\d{2}-\d{4}$", ErrorMessage = "Period must be in MM-YYYY format (e.g. 07-2026).")]
    public string Period {get; set;} = string.Empty;

    [Required]
    [Range(0.01, 9999999, ErrorMessage = "Base amount must be greater than 0.")]
    public decimal BaseAmount {get; set;}

    [Required]
    public DateTime DueDate {get; set;}
}
