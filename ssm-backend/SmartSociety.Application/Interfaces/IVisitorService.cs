using SmartSociety.Domain.Enums;
using SmartSociety.Application.DTOs;

namespace SmartSociety.Application.Interfaces;

public interface IVisitorService
{
    Task<IEnumerable<VisitorResponseDto>> GetAllAsync();
    Task<VisitorResponseDto> GetByIdAsync(Guid id);
    Task<IEnumerable<VisitorResponseDto>> GetByApartmentIdAsync(Guid apartmentId);
    Task<PagedResult<VisitorResponseDto>> GetByApartmentIdPagedAsync(Guid apartmentId, PaginationQuery query);
    Task<IEnumerable<VisitorResponseDto>> GetByStatusAsync(VisitorStatus status);
    Task<IEnumerable<VisitorResponseDto>> GetMyVisitorsAsync(Guid residentUserId);
    Task<IEnumerable<VisitorResponseDto>> GetByUserIdAsync(Guid userId); 
    Task<VisitorResponseDto> RegisterAsync(RegisterVisitorDto dto, Guid residentUserId);
    Task DenyAsync(Guid visitorId);
    Task<VisitorEntryResponseDto> CheckInAsync(string token, Guid staffId); 
    Task<VisitorEntryResponseDto> CheckOutAsync(Guid visitorId, Guid staffId);
    Task<IEnumerable<VisitorEntryResponseDto>> GetEntriesByVisitorIdAsync(Guid visitorId);
}