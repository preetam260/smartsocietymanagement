using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSociety.Application.DTOs;
using SmartSociety.Application.Interfaces;

namespace SmartSociety.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BillController : ControllerBase
{
    private readonly IBillService _billService;

    public BillController(IBillService billService)
    {
        _billService = billService;
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResult<BillResponseDto>>> GetAll([FromQuery] PaginationQuery query)
    {
        var result = await _billService.GetAllPagedAsync(query);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BillResponseDto>> GetById(Guid id)
    {
        var bill = await _billService.GetByIdAsync(id);
        return Ok(bill);
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<BillResponseDto>>> GetPending()
    {
        var bills = await _billService.GetPendingBillsAsync();
        return Ok(bills);
    }

    [HttpGet("apartment/{apartmentId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<BillResponseDto>>> GetByApartmentId(Guid apartmentId)
    {
        var bills = await _billService.GetByApartmentIdAsync(apartmentId);
        return Ok(bills);
    }

    [HttpGet("my")]
    [Authorize(Roles = "Resident,Owner")]
    public async Task<ActionResult<IEnumerable<BillResponseDto>>> GetMyBills()
    {
        var bills = await _billService.GetMyBillsAsync(GetUserId());
        return Ok(bills);
    }

    [HttpGet("period/{period}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<BillResponseDto>>> GetByPeriod(string period)
    {
        var bills = await _billService.GetByPeriodAsync(period);
        return Ok(bills);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BillResponseDto>> Create([FromBody] CreateBillDto dto)
    {
        var bill = await _billService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = bill.Id }, bill);
    }

    [HttpPost("apply-penalties")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApplyPenalties()
    {
        await _billService.ApplyPenaltiesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _billService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("user/{userId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<BillResponseDto>>> GetByUserId(Guid userId)
    {
        var bills = await _billService.GetByUserIdAsync(userId);
        return Ok(bills);
    }
}
