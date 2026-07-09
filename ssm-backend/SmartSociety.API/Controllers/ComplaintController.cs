using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSociety.Application.DTOs;
using SmartSociety.Application.Interfaces;

namespace SmartSociety.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ComplaintController : ControllerBase
{
    private readonly IComplaintService _complaintService;

    public ComplaintController(IComplaintService complaintService)
    {
        _complaintService = complaintService;
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    [Authorize(Roles = "Resident,Owner")]
    public async Task<ActionResult<ComplaintResponseDto>> Create([FromBody] CreateComplaintDto dto)
    {
        var complaint = await _complaintService.CreateAsync(dto, GetUserId());
        return CreatedAtAction(nameof(GetMyComplaints), complaint);
    }

    [HttpGet("my")]
    [Authorize(Roles = "Resident,Owner")]
    public async Task<ActionResult<IEnumerable<ComplaintResponseDto>>> GetMyComplaints()
    {
        var complaints = await _complaintService.GetMyComplaintsAsync(GetUserId());
        return Ok(complaints);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<ComplaintResponseDto>>> GetAll()
    {
        var complaints = await _complaintService.GetAllAsync();
        return Ok(complaints);
    }

    [HttpPatch("{id:guid}/resolve")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ComplaintResponseDto>> Resolve(Guid id, [FromBody] ResolveComplaintDto dto)
    {
        var complaint = await _complaintService.ResolveAsync(id, dto);
        return Ok(complaint);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ComplaintResponseDto>> UpdateStatus(Guid id, [FromBody] UpdateComplaintStatusDto dto)
    {
        var complaint = await _complaintService.UpdateStatusAsync(id, dto);
        return Ok(complaint);
    }
}
