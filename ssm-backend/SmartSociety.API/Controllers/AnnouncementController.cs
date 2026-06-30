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
public class AnnouncementController : ControllerBase
{
    private readonly IAnnouncementService _announcementService;

    public AnnouncementController(IAnnouncementService announcementService)
    {
        _announcementService = announcementService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResult<AnnouncementResponseDto>>> GetAll([FromQuery] PaginationQuery query)
    {
        var result = await _announcementService.GetAllPagedAsync(query);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AnnouncementResponseDto>> GetById(Guid id)
    {
        var announcement = await _announcementService.GetByIdAsync(id);
        return Ok(announcement);
    }

    [HttpGet("audience/{role}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<AnnouncementResponseDto>>> GetByAudience(UserRole role)
    {
        var announcements = await _announcementService.GetActiveByAudienceAsync(role);
        return Ok(announcements);
    }

    [HttpGet("pinned")]
    public async Task<ActionResult<IEnumerable<AnnouncementResponseDto>>> GetPinned()
    {
        var announcements = await _announcementService.GetPinnedAsync();
        return Ok(announcements);
    }

    [HttpGet("mine")]
    public async Task<ActionResult<IEnumerable<AnnouncementResponseDto>>> GetMine()
    {
        var roleClaim = User.FindFirstValue(System.Security.Claims.ClaimTypes.Role);
        if (!Enum.TryParse<UserRole>(roleClaim, out var role))
            return BadRequest(new { message = "Unable to determine your role." });
        var announcements = await _announcementService.GetActiveByAudienceAsync(role);
        return Ok(announcements);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AnnouncementResponseDto>> Create([FromBody] AnnouncementDto dto)
    {
        var announcement = await _announcementService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = announcement.Id }, announcement);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AnnouncementResponseDto>> Update(Guid id, [FromBody] AnnouncementDto dto)
    {
        var announcement = await _announcementService.UpdateAsync(id, dto);
        return Ok(announcement);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _announcementService.DeleteAsync(id);
        return NoContent();
    }
}
