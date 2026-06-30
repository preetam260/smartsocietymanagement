// Services/FacilityService.cs
using SmartSociety.Application.DTOs;
using SmartSociety.Application.Exceptions;
using SmartSociety.Application.Interfaces;
using SmartSociety.Domain.Models;
using SmartSociety.Repository.Interfaces;

namespace SmartSociety.Application.Services;

public class FacilityService : IFacilityService
{
    private readonly IUnitOfWork _uow;

    public FacilityService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<FacilityResponseDto>> GetAllAsync()
    {
        var facilities = await _uow.Facilities.GetAllAsync();
        return facilities.Select(MapToDto);
    }

    public async Task<IEnumerable<FacilityResponseDto>> GetActiveAsync()
    {
        var facilities = await _uow.Facilities.GetActiveFacilitiesAsync();
        return facilities.Select(MapToDto);
    }

    public async Task<FacilityResponseDto> GetByIdAsync(Guid id)
    {
        var facility = await _uow.Facilities.GetByIdAsync(id)
            ?? throw new NotFoundException("Facility", id);
        return MapToDto(facility);
    }

    public async Task<FacilityResponseDto> CreateAsync(FacilityDto dto)
    {
        var facility = new Facility
        {
            Name = dto.Name,
            Description = dto.Description,
            HourlyRate = dto.HourlyRate,
            Capacity = dto.Capacity,
            IsActive = dto.IsActive
        };

        await _uow.Facilities.AddAsync(facility);
        await _uow.SaveChangesAsync();
        return MapToDto(facility);
    }

    public async Task<FacilityResponseDto> UpdateAsync(Guid id, FacilityDto dto)
    {
        var facility = await _uow.Facilities.GetByIdAsync(id)
            ?? throw new NotFoundException("Facility", id);

        facility.Name = dto.Name;
        facility.Description = dto.Description;
        facility.HourlyRate = dto.HourlyRate;
        facility.Capacity = dto.Capacity;
        facility.IsActive = dto.IsActive;
        facility.UpdatedAt = DateTime.UtcNow;

        await _uow.Facilities.UpdateAsync(facility);
        await _uow.SaveChangesAsync();
        return MapToDto(facility);
    }

    public async Task DeleteAsync(Guid id)
    {
        var facility = await _uow.Facilities.GetByIdAsync(id)
            ?? throw new NotFoundException("Facility", id);
        await _uow.Facilities.DeleteAsync(facility);
        await _uow.SaveChangesAsync();
    }

    private static FacilityResponseDto MapToDto(Facility f) => new()
    {
        Id = f.Id,
        Name = f.Name,
        Description = f.Description,
        HourlyRate = f.HourlyRate,
        Capacity = f.Capacity,
        IsActive = f.IsActive
    };
}