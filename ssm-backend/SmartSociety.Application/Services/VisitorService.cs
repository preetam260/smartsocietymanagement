using SmartSociety.Application.DTOs;
using SmartSociety.Application.Exceptions;
using SmartSociety.Application.Interfaces;
using SmartSociety.Domain.Enums;
using SmartSociety.Domain.Models;
using SmartSociety.Repository.Interfaces;

namespace SmartSociety.Application.Services;

public class VisitorService : IVisitorService
{
    private readonly IUnitOfWork _uow;
    private readonly IQRService _qrService;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;

    public VisitorService(
        IUnitOfWork uow,
        IQRService qrService,
        IEmailService emailService,
        INotificationService notificationService)
    {
        _uow = uow;
        _qrService = qrService;
        _emailService = emailService;
        _notificationService = notificationService;
    }

    public async Task<VisitorResponseDto> GetByIdAsync(Guid id)
    {
        var visitor = await _uow.Visitors.GetByIdAsync(id)
            ?? throw new NotFoundException("Visitor", id);
        return await MapToDtoAsync(visitor);
    }

    public async Task<IEnumerable<VisitorResponseDto>> GetByApartmentIdAsync(Guid apartmentId)
    {
        var visitors = await _uow.Visitors.GetByApartmentIdAsync(apartmentId);
        return await MapToDtoListAsync(visitors);
    }

    public async Task<PagedResult<VisitorResponseDto>> GetByApartmentIdPagedAsync(Guid apartmentId, PaginationQuery query)
    {
        var visitors = await _uow.Visitors.GetByApartmentIdAsync(apartmentId);
        var dtos = await MapToDtoListAsync(visitors);
        return PagedResult<VisitorResponseDto>.Create(dtos, query.PageNumber, query.PageSize, query.Search,
            new Func<VisitorResponseDto, string?>[] { v => v.Name, v => v.Email });
    }

    public async Task<IEnumerable<VisitorResponseDto>> GetByStatusAsync(VisitorStatus status)
    {
        var visitors = await _uow.Visitors.GetByStatusAsync(status);
        return await MapToDtoListAsync(visitors);
    }

    public async Task<IEnumerable<VisitorResponseDto>> GetMyVisitorsAsync(Guid residentUserId)
    {
        var resident = await _uow.Residents.GetCurrentByUserIdAsync(residentUserId)
            ?? throw new NotFoundException("No active residency found for this user.");
        var visitors = await _uow.Visitors.GetByApartmentIdAsync(resident.ApartmentId);
        return await MapToDtoListAsync(visitors);
    }

    public async Task<IEnumerable<VisitorResponseDto>> GetByUserIdAsync(Guid userId)
    {
        var resident = await _uow.Residents.GetCurrentByUserIdAsync(userId)
            ?? throw new NotFoundException("No active residency found for this user.");
        var visitors = await _uow.Visitors.GetByApartmentIdAsync(resident.ApartmentId);
        return await MapToDtoListAsync(visitors);
    }

    public async Task<VisitorResponseDto> RegisterAsync(RegisterVisitorDto dto, Guid residentUserId)
    {
        var resident = await _uow.Residents.GetCurrentByUserIdAsync(residentUserId)
            ?? throw new NotFoundException("No active residency found for this user.");

        if (dto.ETA <= DateTime.UtcNow)
            throw new BadRequestException("ETA must be a future date and time.");

        var token = await _qrService.GenerateTokenAsync();

        var visitor = new Visitor
        {
            Name = dto.Name,
            Email = dto.Email,
            Purpose = dto.Purpose,
            ApartmentId = resident.ApartmentId,
            QrToken = token,
            ETA = dto.ETA,
            ExpiresAt = dto.ETA.AddHours(24),
            Status = VisitorStatus.Approved
        };

        await _uow.Visitors.AddAsync(visitor);
        await _uow.SaveChangesAsync();

        var qrImage = await _qrService.GenerateImageAsync(token);
        var apartment = await _uow.Apartments.GetByIdAsync(resident.ApartmentId);

        var emailBody = $@"
            <h2>SmartSociety — Visitor Pass</h2>
            <p>Dear {dto.Name},</p>
            <p>You have been registered as a visitor.</p>
            <table cellpadding='8'>
                <tr><td><strong>Purpose:</strong></td><td>{dto.Purpose}</td></tr>
                <tr><td><strong>Apartment:</strong></td><td>{apartment?.Block}-{apartment?.Number}</td></tr>
                <tr><td><strong>ETA:</strong></td><td>{dto.ETA:dd MMM yyyy, hh:mm tt}</td></tr>
                <tr><td><strong>Valid Until:</strong></td><td>{visitor.ExpiresAt:dd MMM yyyy, hh:mm tt}</td></tr>
            </table>
            <p>Show this QR code at the gate:</p>
            <img src='cid:qrcode' alt='QR Code' style='width:200px;height:200px;' />
            <p><small>Do not share this QR code with anyone.</small></p>";

        await _emailService.SendAsync(dto.Email, "Your SmartSociety Visitor Pass", emailBody, qrImage, "qrcode.png");

        return await MapToDtoAsync(visitor);
    }

    public async Task DenyAsync(Guid visitorId)
    {
        var visitor = await _uow.Visitors.GetByIdAsync(visitorId)
            ?? throw new NotFoundException("Visitor", visitorId);

        if (visitor.Status != VisitorStatus.Approved)
            throw new BadRequestException($"Cannot deny a visitor with status '{visitor.Status}'.");

        visitor.Status = VisitorStatus.Denied;
        visitor.UpdatedAt = DateTime.UtcNow;

        await _uow.Visitors.UpdateAsync(visitor);
        await _uow.SaveChangesAsync();

        await NotifyApartmentResidentsAsync(
            visitor.ApartmentId,
            "Visitor Entry Denied",
            $"Entry for your visitor {visitor.Name} has been denied.");
    }

    public async Task<VisitorEntryResponseDto> CheckInAsync(string token, Guid staffId)
    {
        var visitor = await _uow.Visitors.GetByQrTokenAsync(token)
            ?? throw new NotFoundException("No visitor found for this QR token.");

        if (visitor.Status != VisitorStatus.Approved)
            throw new BadRequestException($"Visitor cannot be checked in. Current status: {visitor.Status}.");

        if (DateTime.UtcNow > visitor.ExpiresAt)
        {
            visitor.Status = VisitorStatus.Expired;
            await _uow.Visitors.UpdateAsync(visitor);
            await _uow.SaveChangesAsync();
            throw new BadRequestException("This visitor pass has expired.");
        }

        var activeEntry = await _uow.VisitorEntries.GetActiveEntryAsync(visitor.Id);
        if (activeEntry != null)
            throw new BadRequestException("Visitor is already checked in.");

        var entry = new VisitorEntry
        {
            VisitorId = visitor.Id,
            CheckinTime = DateTime.UtcNow,
            StaffId = staffId
        };

        visitor.Status = VisitorStatus.CheckedIn;
        visitor.UpdatedAt = DateTime.UtcNow;

        await _uow.VisitorEntries.AddAsync(entry);
        await _uow.Visitors.UpdateAsync(visitor);
        await _uow.SaveChangesAsync();

        await NotifyApartmentResidentsAsync(
            visitor.ApartmentId,
            "Visitor Arrived",
            $"Your visitor {visitor.Name} has checked in at the gate.");

        return await MapEntryToDtoAsync(entry, visitor);
    }

    public async Task<VisitorEntryResponseDto> CheckOutAsync(Guid visitorId, Guid staffId)
    {
        var visitor = await _uow.Visitors.GetByIdAsync(visitorId)
            ?? throw new NotFoundException("Visitor", visitorId);

        if (visitor.Status != VisitorStatus.CheckedIn)
            throw new BadRequestException("Visitor is not currently checked in.");

        var entry = await _uow.VisitorEntries.GetActiveEntryAsync(visitorId)
            ?? throw new NotFoundException("No active entry found for this visitor.");

        entry.CheckoutTime = DateTime.UtcNow;
        entry.UpdatedAt = DateTime.UtcNow;

        visitor.Status = VisitorStatus.CheckedOut;
        visitor.UpdatedAt = DateTime.UtcNow;

        await _uow.VisitorEntries.UpdateAsync(entry);
        await _uow.Visitors.UpdateAsync(visitor);
        await _uow.SaveChangesAsync();

        await NotifyApartmentResidentsAsync(
            visitor.ApartmentId,
            "Visitor Departed",
            $"Your visitor {visitor.Name} has checked out.");

        return await MapEntryToDtoAsync(entry, visitor);
    }

    public async Task<IEnumerable<VisitorEntryResponseDto>> GetEntriesByVisitorIdAsync(Guid visitorId)
    {
        var visitor = await _uow.Visitors.GetByIdAsync(visitorId)
            ?? throw new NotFoundException("Visitor", visitorId);

        var entries = await _uow.VisitorEntries.GetByVisitorIdAsync(visitorId);
        var result = new List<VisitorEntryResponseDto>();
        foreach (var entry in entries)
            result.Add(await MapEntryToDtoAsync(entry, visitor));
        return result;
    }

    private async Task NotifyApartmentResidentsAsync(Guid apartmentId, string title, string message)
    {
        var residents = await _uow.Residents.GetByApartmentIdAsync(apartmentId);
        foreach (var r in residents.Where(r => r.MoveOutDate == null))
            await _notificationService.CreateAsync(r.UserId, title, message);
    }

    private async Task<VisitorResponseDto> MapToDtoAsync(Visitor v)
    {
        var apartment = await _uow.Apartments.GetByIdAsync(v.ApartmentId);
        return new VisitorResponseDto
        {
            Id = v.Id,
            Name = v.Name,
            Email = v.Email,
            Purpose = v.Purpose,
            ApartmentId = v.ApartmentId,
            ApartmentBlock = apartment?.Block ?? "",
            ApartmentNumber = apartment?.Number ?? "",
            QrToken = v.QrToken,
            ETA = v.ETA,
            ExpiresAt = v.ExpiresAt,
            Status = v.Status
        };
    }

    private async Task<IEnumerable<VisitorResponseDto>> MapToDtoListAsync(IEnumerable<Visitor> visitors)
    {
        var result = new List<VisitorResponseDto>();
        foreach (var v in visitors)
            result.Add(await MapToDtoAsync(v));
        return result;
    }

    private async Task<VisitorEntryResponseDto> MapEntryToDtoAsync(VisitorEntry entry, Visitor visitor)
    {
        var staff = await _uow.Users.GetByIdAsync(entry.StaffId);
        return new VisitorEntryResponseDto
        {
            Id = entry.Id,
            VisitorId = entry.VisitorId,
            VisitorName = visitor.Name,
            CheckinTime = entry.CheckinTime,
            CheckoutTime = entry.CheckoutTime,
            StaffId = entry.StaffId,
            StaffName = staff?.Name ?? "Unknown"
        };
    }
}