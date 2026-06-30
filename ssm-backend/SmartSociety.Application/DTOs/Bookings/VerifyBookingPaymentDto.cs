namespace SmartSociety.Application.DTOs;

public class VerifyBookingPaymentDto
{
    public Guid BookingId { get; set; }
    public string OrderId { get; set; } = string.Empty;
    public string PaymentId { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
}
