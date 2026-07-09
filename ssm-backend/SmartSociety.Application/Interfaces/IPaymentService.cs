using SmartSociety.Application.DTOs;
public interface IPaymentService
{
    Task<CreatePaymentOrderResponseDto> CreateOrderAsync(Guid billId, Guid userId);
    Task<BillResponseDto> VerifyAndCompleteAsync(VerifyPaymentDto dto);
}