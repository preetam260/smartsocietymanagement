using SmartSociety.Application.DTOs;
using SmartSociety.Application.Exceptions;
using SmartSociety.Application.Interfaces;
using SmartSociety.Domain.Enums;
using SmartSociety.Domain.Models;
using SmartSociety.Repository.Interfaces;

namespace SmartSociety.Application.Services;

public class ApartmentService : IApartmentService
{
    private readonly IUnitOfWork _uow;

    public ApartmentService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<ApartmentResponseDto>> GetAllAsync()
    {
        var apartments = await _uow.Apartments.GetAllAsync();
        return await MapToDtoListAsync(apartments);
    }

    public async Task<PagedResult<ApartmentResponseDto>> GetAllPagedAsync(PaginationQuery query)
    {
        var apartments = await _uow.Apartments.GetAllAsync();
        var dtos = await MapToDtoListAsync(apartments);
        return PagedResult<ApartmentResponseDto>.Create(dtos, query.PageNumber, query.PageSize, query.Search,
            new Func<ApartmentResponseDto, string?>[] { a => a.Block, a => a.Number });
    }

    public async Task<ApartmentResponseDto> GetByIdAsync(Guid id)
    {
        var apartment = await _uow.Apartments.GetByIdAsync(id)
            ?? throw new NotFoundException("Apartment", id);
        return await MapToDtoAsync(apartment);
    }

    public async Task<IEnumerable<ApartmentResponseDto>> GetByOwnerAsync(Guid ownerId)
    {
        var apartments = await _uow.Apartments.GetByOwnerIdAsync(ownerId);
        return await MapToDtoListAsync(apartments);
    }

    public async Task<ApartmentResponseDto> CreateAsync(CreateApartmentDto dto)
    {
        var existing = await _uow.Apartments.GetByBlockAndNumberAsync(dto.Block, dto.Number);
        if (existing != null)
            throw new ConflictException($"Apartment {dto.Block}-{dto.Number} already exists.");

        var owner = await _uow.Users.GetByIdAsync(dto.OwnerId)
            ?? throw new NotFoundException("User", dto.OwnerId);

        if (owner.Role != UserRole.Owner)
            throw new BadRequestException("The specified user does not have the Owner role.");

        var apartment = new Apartment
        {
            OwnerId = dto.OwnerId,
            Block = dto.Block,
            Floor = dto.Floor,
            Number = dto.Number
        };

        await _uow.Apartments.AddAsync(apartment);
        await _uow.SaveChangesAsync();
        return await MapToDtoAsync(apartment);
    }

    public async Task DeleteAsync(Guid id)
    {
        var apartment = await _uow.Apartments.GetByIdAsync(id)
            ?? throw new NotFoundException("Apartment", id);

        await _uow.Apartments.DeleteAsync(apartment);
        await _uow.SaveChangesAsync();
    }

    private async Task<ApartmentResponseDto> MapToDtoAsync(Apartment a)
    {
        var owner = await _uow.Users.GetByIdAsync(a.OwnerId);
        return new ApartmentResponseDto
        {
            Id = a.Id,
            Block = a.Block,
            Floor = a.Floor,
            Number = a.Number,
            OwnerId = a.OwnerId,
            OwnerName = owner?.Name ?? ""
        };
    }

    private async Task<IEnumerable<ApartmentResponseDto>> MapToDtoListAsync(IEnumerable<Apartment> apartments)
    {
        var result = new List<ApartmentResponseDto>();
        foreach (var a in apartments)
            result.Add(await MapToDtoAsync(a));
        return result;
    }
}