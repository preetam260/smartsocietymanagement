using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSociety.Application.DTOs;
using SmartSociety.Application.Interfaces;

namespace SmartSociety.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ResidentController : ControllerBase
{
    private readonly IResidentService _residentService;

    public ResidentController(IResidentService residentService)
    {
        _residentService = residentService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ResidentResponseDto>>> GetAll()
    {
        var residents = await _residentService.GetAllAsync();
        return Ok(residents);
    }

    [HttpGet("current")]
    public async Task<ActionResult<IEnumerable<ResidentResponseDto>>> GetAllCurrent()
    {
        var residents = await _residentService.GetAllCurrentAsync();
        return Ok(residents);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ResidentResponseDto>> GetById(Guid id)
    {
        var resident = await _residentService.GetByIdAsync(id);
        return Ok(resident);
    }

    [HttpGet("apartment/{apartmentId:guid}")]
    public async Task<ActionResult<IEnumerable<ResidentResponseDto>>> GetByApartmentId(Guid apartmentId)
    {
        var residents = await _residentService.GetByApartmentIdAsync(apartmentId);
        return Ok(residents);
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<ResidentResponseDto>> GetCurrentByUserId(Guid userId)
    {
        var resident = await _residentService.GetCurrentByUserIdAsync(userId);
        return Ok(resident);
    }


    [HttpPost]
    public async Task<ActionResult<ResidentResponseDto>> Create([FromBody] CreateResidentDto dto)
    {
        var resident = await _residentService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = resident.Id }, resident);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ResidentResponseDto>> Update(Guid id, [FromBody] UpdateResidentDto dto)
    {
        var resident = await _residentService.UpdateAsync(id, dto);
        return Ok(resident);
    }

    [HttpPatch("{id:guid}/moveout")]
    public async Task<IActionResult> MoveOut(Guid id, [FromBody] MoveOutResidentDto dto)
    {
        await _residentService.MoveOutAsync(id, dto);
        return NoContent();
    }
}
