using SmartSociety.Application.DTOs;

namespace SmartSociety.Application.Interfaces;
public interface IBillService
{
    Task<IEnumerable<BillResponseDto>> GetAllAsync();
    Task<PagedResult<BillResponseDto>> GetAllPagedAsync(PaginationQuery query);
    Task<BillResponseDto> GetByIdAsync(Guid id);
    Task<IEnumerable<BillResponseDto>> GetPendingBillsAsync();
    Task<IEnumerable<BillResponseDto>> GetByApartmentIdAsync(Guid apartmentId);
    Task<IEnumerable<BillResponseDto>> GetMyBillsAsync(Guid userId);
    Task<IEnumerable<BillResponseDto>> GetByUserIdAsync(Guid userId); // admin use
    Task<IEnumerable<BillResponseDto>> GetByPeriodAsync(string period);
    Task<BillResponseDto> CreateAsync(CreateBillDto dto);
    Task ApplyPenaltiesAsync();
    Task DeleteAsync(Guid id);
}
