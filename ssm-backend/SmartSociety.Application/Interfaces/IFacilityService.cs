using SmartSociety.Application.DTOs;

namespace SmartSociety.Application.Interfaces;

public interface IFacilityService
{
    Task<IEnumerable<FacilityResponseDto>> GetAllAsync();
    Task<IEnumerable<FacilityResponseDto>> GetActiveAsync();
    Task<FacilityResponseDto> GetByIdAsync(Guid id);
    Task<FacilityResponseDto> CreateAsync(FacilityDto dto);
    Task<FacilityResponseDto> UpdateAsync(Guid id, FacilityDto dto);
    Task DeleteAsync(Guid id);
}