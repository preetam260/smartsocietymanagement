using SmartSociety.Domain.Enums;
using SmartSociety.Application.DTOs;

namespace SmartSociety.Application.Interfaces;

public interface IAnnouncementService
{
    Task<IEnumerable<AnnouncementResponseDto>> GetAllAsync();
    Task<PagedResult<AnnouncementResponseDto>> GetAllPagedAsync(PaginationQuery query);
    Task<AnnouncementResponseDto> GetByIdAsync(Guid id);
    Task<IEnumerable<AnnouncementResponseDto>> GetActiveByAudienceAsync(UserRole role);
    Task<IEnumerable<AnnouncementResponseDto>> GetPinnedAsync();
    Task<AnnouncementResponseDto> CreateAsync(AnnouncementDto dto);
    Task<AnnouncementResponseDto> UpdateAsync(Guid id, AnnouncementDto dto);
    Task DeleteAsync(Guid id);
}