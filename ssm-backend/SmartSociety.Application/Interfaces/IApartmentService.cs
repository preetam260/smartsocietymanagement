using SmartSociety.Application.DTOs;

namespace SmartSociety.Application.Interfaces;

public interface IApartmentService
{
    Task<IEnumerable<ApartmentResponseDto>> GetAllAsync();
    Task<PagedResult<ApartmentResponseDto>> GetAllPagedAsync(PaginationQuery query);
    Task<ApartmentResponseDto> GetByIdAsync(Guid id);
    Task<IEnumerable<ApartmentResponseDto>> GetByOwnerAsync(Guid ownerId);
    Task<ApartmentResponseDto> CreateAsync(CreateApartmentDto dto);
    Task DeleteAsync(Guid id);
}