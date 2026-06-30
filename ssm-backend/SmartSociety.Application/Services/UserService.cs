
using SmartSociety.Application.DTOs;
using SmartSociety.Application.Exceptions;
using SmartSociety.Application.Interfaces;
using SmartSociety.Domain.Enums;
using SmartSociety.Domain.Models;
using SmartSociety.Repository.Interfaces;

namespace SmartSociety.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _uow;
    private readonly IEmailService _emailService;

    public UserService(IUnitOfWork uow, IEmailService emailService)
    {
        _uow = uow;
        _emailService = emailService;
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllAsync()
    {
        var users = await _uow.Users.GetAllAsync();
        return users.Select(MapToDto);
    }

    public async Task<PagedResult<UserResponseDto>> GetAllPagedAsync(PaginationQuery query)
    {
        var users = await _uow.Users.GetAllAsync();
        var dtos = users.Select(MapToDto);
        return PagedResult<UserResponseDto>.Create(dtos, query.PageNumber, query.PageSize, query.Search,
            new Func<UserResponseDto, string?>[] { u => u.Name, u => u.Email });
    }

    public async Task<UserResponseDto> GetByIdAsync(Guid id)
    {
        var user = await _uow.Users.GetByIdAsync(id)
            ?? throw new NotFoundException("User", id);
        return MapToDto(user);
    }

    public async Task<IEnumerable<UserResponseDto>> GetByRoleAsync(UserRole role)
    {
        var users = await _uow.Users.GetByRoleAsync(role);
        return users.Select(MapToDto);
    }

    public async Task<UserResponseDto> CreateAsync(CreateUserDto dto)
    {
        var existing = await _uow.Users.GetByEmailAsync(dto.Email);
        if (existing != null)
            throw new ConflictException($"A user with email '{dto.Email}' already exists.");

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = "", // No password — user must use forgot-password to set one
            PhoneNumber = dto.PhoneNumber,
            Role = dto.Role,
            IsActive = true,
        };

        await _uow.Users.AddAsync(user);
        await _uow.SaveChangesAsync();
        await _emailService.SendAsync(
            user.Email,
            "Welcome to SmartSociety",
            $@"
            Hello {user.Name},

            Your SmartSociety account has been created.

            To set your password, please use the Forgot Password feature:
            POST /api/Auth/forgot-password with your email: {user.Email}

            You will receive a reset token via email to set your password.

            Regards,
            SmartSociety Team
        ");
        return MapToDto(user);
    }

    public async Task<UserResponseDto> UpdateAsync(Guid id, UpdateUserDto dto)
    {
        var user = await _uow.Users.GetByIdAsync(id)
            ?? throw new NotFoundException("User", id);

        user.Name = dto.Name;
        user.PhoneNumber = dto.PhoneNumber;
        user.UpdatedAt = DateTime.UtcNow;

        await _uow.Users.UpdateAsync(user);
        await _uow.SaveChangesAsync();
        return MapToDto(user);
    }

    public async Task ActivateAsync(Guid id)
    {
        var user = await _uow.Users.GetByIdAsync(id)
            ?? throw new NotFoundException("User", id);

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _uow.Users.UpdateAsync(user);
        await _uow.SaveChangesAsync();
    }

    public async Task DeactivateAsync(Guid id)
    {
        var user = await _uow.Users.GetByIdAsync(id)
            ?? throw new NotFoundException("User", id);

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _uow.Users.UpdateAsync(user);
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await _uow.Users.GetByIdAsync(id)
            ?? throw new NotFoundException("User", id);

        await _uow.Users.DeleteAsync(user);
        await _uow.SaveChangesAsync();
    }

    private static UserResponseDto MapToDto(User u) => new()
    {
        Id = u.Id,
        Name = u.Name,
        Email = u.Email,
        PhoneNumber = u.PhoneNumber,
        Role = u.Role,
        IsActive = u.IsActive
    };
}