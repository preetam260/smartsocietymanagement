using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSociety.Application.DTOs;
using SmartSociety.Application.Interfaces;

namespace SmartSociety.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FacilityController : ControllerBase
{
    private readonly IFacilityService _facilityService;

    public FacilityController(IFacilityService facilityService)
    {
        _facilityService = facilityService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<FacilityResponseDto>>> GetAll()
    {
        var facilities = await _facilityService.GetAllAsync();
        return Ok(facilities);
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<FacilityResponseDto>>> GetActive()
    {
        var facilities = await _facilityService.GetActiveAsync();
        return Ok(facilities);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FacilityResponseDto>> GetById(Guid id)
    {
        var facility = await _facilityService.GetByIdAsync(id);
        return Ok(facility);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<FacilityResponseDto>> Create([FromBody] FacilityDto dto)
    {
        var facility = await _facilityService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = facility.Id }, facility);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<FacilityResponseDto>> Update(Guid id, [FromBody] FacilityDto dto)
    {
        var facility = await _facilityService.UpdateAsync(id, dto);
        return Ok(facility);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _facilityService.DeleteAsync(id);
        return NoContent();
    }
}
