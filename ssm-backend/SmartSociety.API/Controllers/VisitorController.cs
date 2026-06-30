using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSociety.Application.DTOs;
using SmartSociety.Application.Interfaces;
using SmartSociety.Domain.Enums;

namespace SmartSociety.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VisitorController : ControllerBase
{
    private readonly IVisitorService _visitorService;
    private readonly IQRService _qrService;

    public VisitorController(IVisitorService visitorService, IQRService qrService)
    {
        _visitorService = visitorService;
        _qrService = qrService;
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,SecurityStaff")]
    public async Task<ActionResult<VisitorResponseDto>> GetById(Guid id)
    {
        var visitor = await _visitorService.GetByIdAsync(id);
        return Ok(visitor);
    }

    [HttpGet("apartment/{apartmentId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResult<VisitorResponseDto>>> GetByApartmentId(Guid apartmentId, [FromQuery] PaginationQuery query)
    {
        var result = await _visitorService.GetByApartmentIdPagedAsync(apartmentId, query);
        return Ok(result);
    }

    [HttpGet("status/{status}")]
    [Authorize(Roles = "Admin,SecurityStaff")]
    public async Task<ActionResult<IEnumerable<VisitorResponseDto>>> GetByStatus(VisitorStatus status)
    {
        var visitors = await _visitorService.GetByStatusAsync(status);
        return Ok(visitors);
    }

    [HttpGet("my")]
    [Authorize(Roles = "Resident,Owner")]
    public async Task<ActionResult<IEnumerable<VisitorResponseDto>>> GetMyVisitors()
    {
        var visitors = await _visitorService.GetMyVisitorsAsync(GetUserId());
        return Ok(visitors);
    }

    [HttpPost("register")]
    [Authorize(Roles = "Resident,Owner")]
    public async Task<ActionResult<VisitorResponseDto>> Register([FromBody] RegisterVisitorDto dto)
    {
        var visitor = await _visitorService.RegisterAsync(dto, GetUserId());
        return CreatedAtAction(nameof(GetById), new { id = visitor.Id }, visitor);
    }

    [HttpPatch("{id:guid}/deny")]
    [Authorize(Roles = "Admin,SecurityStaff")]
    public async Task<IActionResult> Deny(Guid id)
    {
        await _visitorService.DenyAsync(id);
        return NoContent();
    }


    [HttpPost("checkin")]
    [Authorize(Roles = "SecurityStaff")]
    public async Task<ActionResult<VisitorEntryResponseDto>> CheckIn([FromForm] CheckInVisitorDto dto)
    {
        IFormFile? finalImageFile = dto.QrToken;

        if (finalImageFile == null || finalImageFile.Length == 0)
            return BadRequest(new { message = "QR code image is required." });

        var token = await _qrService.DecodeImageAsync(finalImageFile);
        var entry = await _visitorService.CheckInAsync(token, GetUserId());
        return Ok(entry);
    }

    [HttpPatch("{id:guid}/checkout")]
    [Authorize(Roles = "SecurityStaff")]
    public async Task<ActionResult<VisitorEntryResponseDto>> CheckOut(Guid id)
    {
        var entry = await _visitorService.CheckOutAsync(id, GetUserId());
        return Ok(entry);
    }

    [HttpGet("{id:guid}/entries")]
    [Authorize(Roles = "Admin,SecurityStaff")]
    public async Task<ActionResult<IEnumerable<VisitorEntryResponseDto>>> GetEntries(Guid id)
    {
        var entries = await _visitorService.GetEntriesByVisitorIdAsync(id);
        return Ok(entries);
    }

    [HttpGet("user/{userId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<VisitorResponseDto>>> GetByUserId(Guid userId)
    {
        var visitors = await _visitorService.GetByUserIdAsync(userId);
        return Ok(visitors);
    }
}
