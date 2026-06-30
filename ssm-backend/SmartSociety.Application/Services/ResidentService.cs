using SmartSociety.Application.DTOs;
using SmartSociety.Application.Exceptions;
using SmartSociety.Application.Interfaces;
using SmartSociety.Domain.Enums;
using SmartSociety.Domain.Models;
using SmartSociety.Repository.Interfaces;

namespace SmartSociety.Application.Services;

public class ResidentService : IResidentService
{
    private readonly IUnitOfWork _uow;

    public ResidentService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<ResidentResponseDto>> GetAllAsync()
    {
        var residents = await _uow.Residents.GetAllAsync();
        return await MapToDtoListAsync(residents);
    }

    public async Task<IEnumerable<ResidentResponseDto>> GetAllCurrentAsync()
    {
        var residents = await _uow.Residents.GetAllCurrentAsync();
        return await MapToDtoListAsync(residents);
    }

    public async Task<ResidentResponseDto> GetByIdAsync(Guid id)
    {
        var resident = await _uow.Residents.GetByIdAsync(id)
            ?? throw new NotFoundException("Resident", id);
        return await MapToDtoAsync(resident);
    }

    public async Task<IEnumerable<ResidentResponseDto>> GetByApartmentIdAsync(Guid apartmentId)
    {
        var residents = await _uow.Residents.GetByApartmentIdAsync(apartmentId);
        return await MapToDtoListAsync(residents);
    }

    public async Task<ResidentResponseDto> GetCurrentByUserIdAsync(Guid userId)
    {
        var resident = await _uow.Residents.GetCurrentByUserIdAsync(userId)
            ?? throw new NotFoundException("No active residency found for this user.");
        return await MapToDtoAsync(resident);
    }

    public async Task<ResidentResponseDto> CreateAsync(CreateResidentDto dto)
    {
        var user = await _uow.Users.GetByIdAsync(dto.UserId)
            ?? throw new NotFoundException("User", dto.UserId);

        _ = await _uow.Apartments.GetByIdAsync(dto.ApartmentId)
            ?? throw new NotFoundException("Apartment", dto.ApartmentId);

        if (user.Role != UserRole.Resident && user.Role != UserRole.Owner)
            throw new BadRequestException("Only users with the Resident or Owner role can be added as residents.");

        // Prevent a user from having two active tenancies at the same time
        var existingResidency = await _uow.Residents.GetCurrentByUserIdAsync(dto.UserId);
        if (existingResidency != null)
            throw new ConflictException("This user already has an active residency.");

        var resident = new Resident
        {
            UserId = dto.UserId,
            ApartmentId = dto.ApartmentId,
            MoveInDate = dto.MoveInDate,
            VehicleNumber = dto.VehicleNumber
        };

        await _uow.Residents.AddAsync(resident);
        await _uow.SaveChangesAsync();
        return await MapToDtoAsync(resident);
    }

    public async Task<ResidentResponseDto> UpdateAsync(Guid id, UpdateResidentDto dto)
    {
        var resident = await _uow.Residents.GetByIdAsync(id)
            ?? throw new NotFoundException("Resident", id);

        resident.VehicleNumber = dto.VehicleNumber;
        resident.UpdatedAt = DateTime.UtcNow;

        await _uow.Residents.UpdateAsync(resident);
        await _uow.SaveChangesAsync();
        return await MapToDtoAsync(resident);
    }

    public async Task MoveOutAsync(Guid id, MoveOutResidentDto dto)
    {
        var resident = await _uow.Residents.GetByIdAsync(id)
            ?? throw new NotFoundException("Resident", id);

        if (resident.MoveOutDate != null)
            throw new BadRequestException("This resident has already moved out.");

        if (dto.MoveOutDate < resident.MoveInDate)
            throw new BadRequestException("Move out date cannot be before move in date.");

        var apartment = await _uow.Apartments.GetByIdAsync(resident.ApartmentId)
            ?? throw new NotFoundException("Apartment", resident.ApartmentId);

        // Transfer unpaid/overdue maintenance bills to the apartment owner
        var bills = await _uow.Bills.GetByApartmentIdAsync(resident.ApartmentId);
        var transferStatuses = new[] { BillingStatus.Unpaid, BillingStatus.Overdue };
        foreach (var bill in bills.Where(b => transferStatuses.Contains(b.Status)))
        {
            bill.BilledToUserId = apartment.OwnerId;
            bill.UpdatedAt = DateTime.UtcNow;
            await _uow.Bills.UpdateAsync(bill);
        }

        resident.MoveOutDate = dto.MoveOutDate;
        resident.UpdatedAt = DateTime.UtcNow;

        await _uow.Residents.UpdateAsync(resident);
        await _uow.SaveChangesAsync();
    }

    private async Task<ResidentResponseDto> MapToDtoAsync(Resident r)
    {
        var user = await _uow.Users.GetByIdAsync(r.UserId);
        var apartment = await _uow.Apartments.GetByIdAsync(r.ApartmentId);
        return new ResidentResponseDto
        {
            Id = r.Id,
            UserId = r.UserId,
            UserName = user?.Name ?? "",
            UserEmail = user?.Email ?? "",
            ApartmentId = r.ApartmentId,
            ApartmentBlock = apartment?.Block ?? "",
            ApartmentNumber = apartment?.Number ?? "",
            MoveInDate = r.MoveInDate,
            MoveOutDate = r.MoveOutDate,
            VehicleNumber = r.VehicleNumber
        };
    }

    private async Task<IEnumerable<ResidentResponseDto>> MapToDtoListAsync(IEnumerable<Resident> residents)
    {
        var result = new List<ResidentResponseDto>();
        foreach (var r in residents)
            result.Add(await MapToDtoAsync(r));
        return result;
    }
}