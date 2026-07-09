using SmartSociety.Application.DTOs;

namespace SmartSociety.Application.Interfaces;

public interface IComplaintService
{
    Task<ComplaintResponseDto> CreateAsync(CreateComplaintDto dto, Guid userId);
    Task<IEnumerable<ComplaintResponseDto>> GetMyComplaintsAsync(Guid userId);
    Task<IEnumerable<ComplaintResponseDto>> GetAllAsync();
    Task<ComplaintResponseDto> ResolveAsync(Guid id, ResolveComplaintDto dto);
    Task<ComplaintResponseDto> UpdateStatusAsync(Guid id, UpdateComplaintStatusDto dto);
}
