using System.ComponentModel.DataAnnotations;
using SmartSociety.Domain.Enums;

namespace SmartSociety.Application.DTOs;

public class UpdateComplaintStatusDto
{
    [Required]
    [EnumDataType(typeof(ComplaintStatus))]
    public ComplaintStatus Status {get; set;}

    [MaxLength(2000)]
    public string? AdminResponse {get; set;}
}
