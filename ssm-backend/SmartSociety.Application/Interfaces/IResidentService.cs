using SmartSociety.Application.DTOs;

namespace SmartSociety.Application.Interfaces;

public interface IResidentService
{
    Task<IEnumerable<ResidentResponseDto>> GetAllAsync();
    Task<IEnumerable<ResidentResponseDto>> GetAllCurrentAsync();
    Task<ResidentResponseDto> GetByIdAsync(Guid id);
    Task<IEnumerable<ResidentResponseDto>> GetByApartmentIdAsync(Guid apartmentId);
    Task<ResidentResponseDto> GetCurrentByUserIdAsync(Guid userId);  // resident views own apartment
    Task<ResidentResponseDto> CreateAsync(CreateResidentDto dto);
    Task<ResidentResponseDto> UpdateAsync(Guid id, UpdateResidentDto dto);
    Task MoveOutAsync(Guid id, MoveOutResidentDto dto);
}