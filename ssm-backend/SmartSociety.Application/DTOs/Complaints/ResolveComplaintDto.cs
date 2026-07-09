using System.ComponentModel.DataAnnotations;

namespace SmartSociety.Application.DTOs;

public class ResolveComplaintDto
{
    [Required]
    [MaxLength(2000)]
    public string AdminResponse {get; set;} = string.Empty;
}
