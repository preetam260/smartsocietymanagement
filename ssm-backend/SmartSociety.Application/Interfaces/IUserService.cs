using SmartSociety.Domain.Enums;
using SmartSociety.Application.DTOs;

namespace SmartSociety.Application.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserResponseDto>> GetAllAsync();
    Task<PagedResult<UserResponseDto>> GetAllPagedAsync(PaginationQuery query);
    Task<UserResponseDto> GetByIdAsync(Guid id);
    Task<IEnumerable<UserResponseDto>> GetByRoleAsync(UserRole role);
    Task<UserResponseDto> CreateAsync(CreateUserDto dto);
    Task<UserResponseDto> UpdateAsync(Guid id, UpdateUserDto dto);
    Task ActivateAsync(Guid id);
    Task DeactivateAsync(Guid id);
    Task DeleteAsync(Guid id);


}