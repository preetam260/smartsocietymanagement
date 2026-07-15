using SmartSociety.Application.DTOs;
namespace SmartSociety.Application.Interfaces;

public interface IPaymentService
{
    Task<CreatePaymentOrderResponseDto> CreateOrderAsync(Guid billId, Guid userId);

    Task<BillResponseDto> CompleteSimulatedPaymentAsync(CompletePaymentDto dto, Guid userId);
}