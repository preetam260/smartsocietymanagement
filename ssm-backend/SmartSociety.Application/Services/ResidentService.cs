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
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;

    public ResidentService(
        IUnitOfWork uow,
        INotificationService notificationService,
        IEmailService emailService)
    {
        _uow = uow;
        _notificationService = notificationService;
        _emailService = emailService;
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

        var user = await _uow.Users.GetByIdAsync(resident.UserId)
            ?? throw new NotFoundException("User", resident.UserId);

        var allResidentsInApartment = await _uow.Residents.GetByApartmentIdAsync(resident.ApartmentId);
        var otherActiveResidents = allResidentsInApartment
            .Where(r => r.Id != resident.Id && r.MoveOutDate == null)
            .ToList();

        var allBills = await _uow.Bills.GetByApartmentIdAsync(resident.ApartmentId);
        var residentBills = allBills.Where(b => b.BilledToUserId == resident.UserId).ToList();

        if (!otherActiveResidents.Any())
        {
            var transferStatuses = new[] { BillingStatus.Unpaid, BillingStatus.Overdue };
            foreach (var bill in residentBills.Where(b => transferStatuses.Contains(b.Status)))
            {
                bill.BilledToUserId = apartment.OwnerId;
                bill.UpdatedAt = DateTime.UtcNow;
                await _uow.Bills.UpdateAsync(bill);
            }

            await _notificationService.CreateAsync(
                apartment.OwnerId,
                "Resident Moved Out",
                $"The last resident of apartment {apartment.Block}-{apartment.Number} has moved out on {dto.MoveOutDate:dd MMM yyyy}. " +
                $"Any pending maintenance bills have been transferred to you.");
        }

        resident.MoveOutDate = dto.MoveOutDate;
        resident.UpdatedAt = DateTime.UtcNow;
        await _uow.Residents.UpdateAsync(resident);

        if (user.Role == UserRole.Resident)
        {
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            await _uow.Users.UpdateAsync(user);
        }

        await _uow.SaveChangesAsync();

        await SendBillStatementEmailAsync(user, apartment, resident, residentBills);
    }
    private async Task SendBillStatementEmailAsync(
        User user,
        Apartment apartment,
        Resident resident,
        List<Domain.Models.Bill> bills)
    {
        try
        {
            var billDtos = new List<BillResponseDto>();
            foreach (var b in bills)
            {
                var apt = await _uow.Apartments.GetByIdAsync(b.ApartmentId);
                billDtos.Add(new BillResponseDto
                {
                    Id = b.Id,
                    ApartmentId = b.ApartmentId,
                    ApartmentBlock = apt?.Block ?? "",
                    ApartmentNumber = apt?.Number ?? "",
                    BilledToUserId = user.Id,
                    BilledToUserName = user.Name,
                    Period = b.Period,
                    BaseAmount = b.BaseAmount,
                    Penalty = b.Penalty,
                    Total = b.Total,
                    DueDate = b.DueDate,
                    PaidAt = b.PaidAt,
                    Status = b.Status,
                    TransactionRef = b.TransactionRef,
                    IsVacantRate = b.IsVacantRate,
                });
            }

            var pdfBytes = BillStatementPdfGenerator.Generate(
                residentName:   user.Name,
                residentEmail:  user.Email,
                apartmentRef:   $"{apartment.Block}-{apartment.Number}",
                moveInDate:     resident.MoveInDate,
                moveOutDate:    resident.MoveOutDate!.Value,
                bills:          billDtos);

            var paidCount   = billDtos.Count(b => b.Status == BillingStatus.Paid);
            var unpaidTotal = billDtos.Where(b => b.Status is BillingStatus.Unpaid or BillingStatus.Overdue)
                                    .Sum(b => b.Total);

            var htmlBody = $@"
<div style=""font-family:Arial,sans-serif;max-width:600px;margin:auto"">
<div style=""background:#1e40af;padding:24px 32px"">
    <h2 style=""color:#fff;margin:0"">SmartSociety</h2>
    <p style=""color:#bfdbfe;margin:4px 0 0"">Move-Out Confirmation &amp; Bill Statement</p>
</div>
<div style=""padding:32px"">
    <p>Dear <strong>{user.Name}</strong>,</p>
    <p>Your move-out from apartment <strong>{apartment.Block}-{apartment.Number}</strong>
    has been recorded on <strong>{resident.MoveOutDate:dd MMM yyyy}</strong>.</p>
    <p>Please find your complete maintenance bill statement attached as a PDF.</p>
    <table style=""width:100%;border-collapse:collapse;margin:20px 0"">
    <tr style=""background:#f3f4f6"">
        <td style=""padding:10px 14px;font-weight:bold"">Total Bills</td>
        <td style=""padding:10px 14px"">{billDtos.Count}</td>
    </tr>
    <tr>
        <td style=""padding:10px 14px;font-weight:bold"">Paid Bills</td>
        <td style=""padding:10px 14px;color:#15803d"">{paidCount}</td>
    </tr>
    {(unpaidTotal > 0 ? $@"
    <tr style=""background:#fef2f2"">
        <td style=""padding:10px 14px;font-weight:bold;color:#b91c1c"">Outstanding Amount</td>
        <td style=""padding:10px 14px;color:#b91c1c;font-weight:bold"">₹{unpaidTotal:N2}</td>
    </tr>" : "")}
    </table>
    {(unpaidTotal > 0 ? "<p style=\"color:#b91c1c\"><strong>⚠ Note:</strong> Outstanding bills have been transferred to the apartment owner.</p>" : "<p style=\"color:#15803d\">✅ All bills are cleared. Thank you!</p>")}
    <p style=""color:#6b7280;font-size:13px"">Your account access has been deactivated. Please contact the administrator if you have any questions.</p>
    <p>Thank you for being part of SmartSociety.</p>
    <p>Regards,<br><strong>SmartSociety Management Team</strong></p>
</div>
</div>";

            await _emailService.SendAsync(
                to:              user.Email,
                subject:         "SmartSociety — Move-Out Confirmation & Bill Statement",
                body:            htmlBody,
                attachmentBytes: pdfBytes,
                attachmentName:  $"BillStatement_{user.Name.Replace(" ", "_")}_{resident.MoveOutDate!.Value:yyyyMMdd}.pdf");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ResidentService] Failed to send move-out email to {user.Email}: {ex.Message}");
            throw;
        }
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