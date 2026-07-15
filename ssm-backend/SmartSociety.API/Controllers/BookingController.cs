using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSociety.Application.DTOs;
using SmartSociety.Application.Interfaces;

namespace SmartSociety.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResult<BookingResponseDto>>> GetAll([FromQuery] PaginationQuery query)
    {
        var result = await _bookingService.GetAllPagedAsync(query);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Resident,Owner")]
    public async Task<ActionResult<BookingResponseDto>> GetById(Guid id)
    {
        var booking = await _bookingService.GetByIdAsync(id);
        // Non-admin users can only view their own bookings
        if (!User.IsInRole("Admin") && booking.UserId != GetUserId())
            return Forbid();
        return Ok(booking);
    }

    [HttpGet("facility/{facilityId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResult<BookingResponseDto>>> GetByFacilityId(Guid facilityId, [FromQuery] PaginationQuery query)
    {
        var result = await _bookingService.GetByFacilityIdPagedAsync(facilityId, query);
        return Ok(result);
    }

    [HttpGet("my")]
    [Authorize(Roles = "Resident,Owner")]
    public async Task<ActionResult<IEnumerable<BookingResponseDto>>> GetMyBookings()
    {
        var bookings = await _bookingService.GetMyBookingsAsync(GetUserId());
        return Ok(bookings);
    }

    [HttpGet("user/{userId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<BookingResponseDto>>> GetByUserId(Guid userId)
    {
        var bookings = await _bookingService.GetByUserIdAsync(userId);
        return Ok(bookings);
    }

    [HttpGet("calendar")]
    [Authorize(Roles = "Admin,Resident,Owner")]
    public async Task<ActionResult<BookingCalendarResponseDto>> GetCalendar(
        [FromQuery] Guid facilityId,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        if (!User.IsInRole("Admin") &&
            !await _bookingService.HasActiveResidencyAsync(GetUserId()))
            return Forbid();

        var calendar = await _bookingService.GetCalendarAsync(facilityId, from, to);
        return Ok(calendar);
    }

    [HttpPost]
    [Authorize(Roles = "Resident,Owner")]

    public async Task<ActionResult<BookingResponseDto>> Create([FromBody] CreateBookingDto dto)
    {
        var booking = await _bookingService.CreateAsync(dto, GetUserId());
        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
    }

    [HttpPost("{id:guid}/create-order")]
    [Authorize(Roles = "Resident,Owner")]
    public async Task<ActionResult<CreatePaymentOrderResponseDto>> CreatePaymentOrder(Guid id)
    {
        var order = await _bookingService.CreatePaymentOrderAsync(id, GetUserId());
        return Ok(order);
    }

    [HttpPost("{id:guid}/complete-payment")]
    [Authorize(Roles = "Resident,Owner")]
    public async Task<ActionResult<BookingResponseDto>> CompleteSimulatedPayment(
        Guid id,
        [FromBody] CompleteBookingPaymentDto dto)
    {
        var booking =
            await _bookingService.CompleteSimulatedPaymentAsync(id, dto, GetUserId());

        return Ok(booking);
    }

    [HttpPatch("{id:guid}/cancel")]
    [Authorize(Roles = "Resident,Owner")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        await _bookingService.CancelAsync(id, GetUserId());
        return NoContent();
    }

    [HttpPatch("{id:guid}/complete")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Complete(Guid id)
    {
        await _bookingService.CompleteAsync(id);
        return NoContent();
    }

    [HttpPost("expire-holds")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ExpireHolds()
    {
        await _bookingService.ExpireHoldsAsync();
        return NoContent();
    }
}
