using SmartSociety.Application.DTOs;
using SmartSociety.Application.Exceptions;
using SmartSociety.Application.Interfaces;
using SmartSociety.Domain.Enums;
using SmartSociety.Domain.Models;
using SmartSociety.Repository.Interfaces;

namespace SmartSociety.Application.Services;

public class AnnouncementService : IAnnouncementService
{
    private readonly IUnitOfWork _uow;
    public AnnouncementService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<AnnouncementResponseDto>> GetAllAsync()
    {
        var announcements = await _uow.Announcements.GetAllAsync();
        return announcements.Select(MapToDto);
    }

    public async Task<PagedResult<AnnouncementResponseDto>> GetAllPagedAsync(PaginationQuery query)
    {
        var announcements = await _uow.Announcements.GetAllAsync();
        var dtos = announcements.Select(MapToDto);
        return PagedResult<AnnouncementResponseDto>.Create(dtos, query.PageNumber, query.PageSize, query.Search,
            new Func<AnnouncementResponseDto, string?>[] { a => a.Title, a => a.Content });
    }

    public async Task<AnnouncementResponseDto> GetByIdAsync(Guid id)
    {
        var announcement = await _uow.Announcements.GetByIdAsync(id)
            ?? throw new NotFoundException("Announcement", id);
        return MapToDto(announcement);
    }

    public async Task<IEnumerable<AnnouncementResponseDto>> GetActiveByAudienceAsync(UserRole role)
    {
        var announcements = await _uow.Announcements.GetActiveByAudienceAsync(role);
        return announcements.Select(MapToDto);
    }

    public async Task<IEnumerable<AnnouncementResponseDto>> GetPinnedAsync()
    {
        var announcements = await _uow.Announcements.GetPinnedAsync();
        return announcements.Select(MapToDto);
    }

    public async Task<AnnouncementResponseDto> CreateAsync(AnnouncementDto dto)
    {
        if (dto.ExpiresAt <= DateTime.UtcNow)
            throw new BadRequestException("Expiry date must be in the future.");

        var announcement = new Announcement
        {
            Title = dto.Title,
            Content = dto.Content,
            Audience = dto.Audience,
            IsPinned = dto.IsPinned,
            ExpiresAt = dto.ExpiresAt
        };

        await _uow.Announcements.AddAsync(announcement);
        await _uow.SaveChangesAsync();
        return MapToDto(announcement);
    }

    public async Task<AnnouncementResponseDto> UpdateAsync(Guid id, AnnouncementDto dto)
    {
        var announcement = await _uow.Announcements.GetByIdAsync(id)
            ?? throw new NotFoundException("Announcement", id);

        if (dto.ExpiresAt <= DateTime.UtcNow)
            throw new BadRequestException("Expiry date must be in the future.");

        announcement.Title = dto.Title;
        announcement.Content = dto.Content;
        announcement.Audience = dto.Audience;
        announcement.IsPinned = dto.IsPinned;
        announcement.ExpiresAt = dto.ExpiresAt;
        announcement.UpdatedAt = DateTime.UtcNow;

        await _uow.Announcements.UpdateAsync(announcement);
        await _uow.SaveChangesAsync();
        return MapToDto(announcement);
    }

    public async Task DeleteAsync(Guid id)
    {
        var announcement = await _uow.Announcements.GetByIdAsync(id)
            ?? throw new NotFoundException("Announcement", id);
        await _uow.Announcements.DeleteAsync(announcement);
        await _uow.SaveChangesAsync();
    }

    private static AnnouncementResponseDto MapToDto(Announcement a) => new()
    {
        Id = a.Id,
        Title = a.Title,
        Content = a.Content,
        Audience = a.Audience,
        IsPinned = a.IsPinned,
        ExpiresAt = a.ExpiresAt,
        CreatedAt = a.CreatedAt
    };
}