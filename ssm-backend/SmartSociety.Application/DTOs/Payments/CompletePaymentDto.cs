using System.ComponentModel.DataAnnotations;

namespace SmartSociety.Application.DTOs;

public class CompletePaymentDto
{
    [Required]
    public string OrderId { get; set; } = string.Empty;
}