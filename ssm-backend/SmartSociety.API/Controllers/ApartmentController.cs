using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSociety.Application.DTOs;
using SmartSociety.Application.Interfaces;

namespace SmartSociety.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ApartmentController : ControllerBase
{
    private readonly IApartmentService _apartmentService;

    public ApartmentController(IApartmentService apartmentService)
    {
        _apartmentService = apartmentService;
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<PagedResult<ApartmentResponseDto>>> GetAll([FromQuery] PaginationQuery query)
    {
        var result = await _apartmentService.GetAllPagedAsync(query);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Owner,Resident")]
    public async Task<ActionResult<ApartmentResponseDto>> GetById(Guid id)
    {
        var apartment = await _apartmentService.GetByIdAsync(id);
        
        if (User.IsInRole("Owner") && apartment.OwnerId != GetUserId())
            return Forbid();

        return Ok(apartment);
    }

    [HttpGet("my")]
    [Authorize(Roles = "Owner")]
    public async Task<ActionResult<IEnumerable<ApartmentResponseDto>>> GetMyApartments()
    {
        var apartments = await _apartmentService.GetByOwnerAsync(GetUserId());
        return Ok(apartments);
    }

    [HttpPost]
    public async Task<ActionResult<ApartmentResponseDto>> Create([FromBody] CreateApartmentDto dto)
    {
        var apartment = await _apartmentService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = apartment.Id }, apartment);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _apartmentService.DeleteAsync(id);
        return NoContent();
    }
}
