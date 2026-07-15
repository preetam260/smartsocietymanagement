using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSociety.Application.DTOs;
using SmartSociety.Application.Interfaces;

namespace SmartSociety.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Resident,Owner")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("create-order/{billId:guid}")]
    public async Task<ActionResult<CreatePaymentOrderResponseDto>> CreateOrder(
        Guid billId)
    {
        var order = await _paymentService.CreateOrderAsync(
            billId,
            GetUserId());

        return Ok(order);
    }

    [HttpPost("complete")]
    public async Task<ActionResult<BillResponseDto>> CompleteSimulatedPayment(
        [FromBody] CompletePaymentDto dto)
    {
        var result = await _paymentService.CompleteSimulatedPaymentAsync(
            dto,
            GetUserId());

        return Ok(result);
    }
}