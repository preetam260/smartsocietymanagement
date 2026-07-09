using System.ComponentModel.DataAnnotations;

namespace SmartSociety.Application.DTOs;

public class VerifyBookingPaymentDto
{
    [Required]
    public Guid BookingId {get; set;}

    [Required]
    public string OrderId {get; set;} = string.Empty;

    [Required]
    public string PaymentId {get; set;} = string.Empty;

    [Required]
    public string Signature {get; set;} = string.Empty;
}
