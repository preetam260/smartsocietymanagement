using SmartSociety.Application.DTOs;
using SmartSociety.Application.Exceptions;
using SmartSociety.Application.Interfaces;
using SmartSociety.Domain.Enums;
using SmartSociety.Domain.Models;
using SmartSociety.Repository.Interfaces;

namespace SmartSociety.Application.Services;

public class BillService : IBillService
{
    private readonly IUnitOfWork _uow;
    private readonly INotificationService _notificationService;

    public BillService(IUnitOfWork uow, INotificationService notificationService)
    {
        _uow = uow;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<BillResponseDto>> GetAllAsync()
    {
        var bills = await _uow.Bills.GetAllAsync();
        return await MapToDtoListAsync(bills);
    }

    public async Task<PagedResult<BillResponseDto>> GetAllPagedAsync(PaginationQuery query)
    {
        var bills = await _uow.Bills.GetAllAsync();
        var dtos = await MapToDtoListAsync(bills);
        return PagedResult<BillResponseDto>.Create(dtos, query.PageNumber, query.PageSize, query.Search,
            new Func<BillResponseDto, string?>[] { b => b.Period, b => b.ApartmentBlock, b => b.ApartmentNumber });
    }

    public async Task<BillResponseDto> GetByIdAsync(Guid id)
    {
        var bill = await _uow.Bills.GetByIdAsync(id)
            ?? throw new NotFoundException("Bill", id);
        return await MapToDtoAsync(bill);
    }

    public async Task<IEnumerable<BillResponseDto>> GetPendingBillsAsync()
    {
        var bills = await _uow.Bills.GetPendingBillsAsync();
        return await MapToDtoListAsync(bills);
    }

    public async Task<IEnumerable<BillResponseDto>> GetByApartmentIdAsync(Guid apartmentId)
    {
        var bills = await _uow.Bills.GetByApartmentIdAsync(apartmentId);
        return await MapToDtoListAsync(bills);
    }

    public async Task<IEnumerable<BillResponseDto>> GetMyBillsAsync(Guid userId)
    {
        var allBills = new List<Bill>();

        var resident = await _uow.Residents.GetCurrentByUserIdAsync(userId);
        if (resident != null)
        {
            var bills = await _uow.Bills.GetByApartmentIdAsync(resident.ApartmentId);
            allBills.AddRange(bills);
        }

        var user = await _uow.Users.GetByIdAsync(userId);
        if (user != null && user.Role == UserRole.Owner)
        {
            var apartments = await _uow.Apartments.GetByOwnerIdAsync(userId);
            foreach (var apt in apartments)
            {
                var bills = await _uow.Bills.GetByApartmentIdAsync(apt.Id);
                foreach (var b in bills)
                {
                    if (!allBills.Any(x => x.Id == b.Id))
                        allBills.Add(b);
                }
            }
        }

        return await MapToDtoListAsync(allBills.OrderByDescending(b => b.DueDate));
    }

    public async Task<IEnumerable<BillResponseDto>> GetByUserIdAsync(Guid userId)
    {
        var resident = await _uow.Residents.GetCurrentByUserIdAsync(userId)
            ?? throw new NotFoundException("No active residency found for this user.");
        var bills = await _uow.Bills.GetByApartmentIdAsync(resident.ApartmentId);
        return await MapToDtoListAsync(bills);
    }

    public async Task<IEnumerable<BillResponseDto>> GetByPeriodAsync(string period)
    {
        var bills = await _uow.Bills.GetByPeriodAsync(period);
        return await MapToDtoListAsync(bills);
    }

    public async Task<BillResponseDto> CreateAsync(CreateBillDto dto)
    {
        var apartment = await _uow.Apartments.GetByIdAsync(dto.ApartmentId)
            ?? throw new NotFoundException("Apartment", dto.ApartmentId);

        var existing = await _uow.Bills.GetByApartmentAndPeriodAsync(dto.ApartmentId, dto.Period);
        if (existing != null)
            throw new ConflictException($"A bill already exists for {apartment.Block}-{apartment.Number} for period {dto.Period}.");

        if (dto.DueDate <= DateTime.UtcNow)
            throw new BadRequestException("Due date must be in the future.");

        var residents = await _uow.Residents.GetByApartmentIdAsync(dto.ApartmentId);
        var currentResident = residents.FirstOrDefault(r => r.MoveOutDate == null);

        Guid billedToUserId;
        decimal baseAmount = dto.BaseAmount;
        bool isVacantRate = false;

        if (currentResident != null)
        {
            billedToUserId = currentResident.UserId;
        }
        else
        {
            billedToUserId = apartment.OwnerId;
            baseAmount = Math.Round(dto.BaseAmount * 0.25m, 2);
            isVacantRate = true;
        }

        var bill = new Bill
        {
            ApartmentId = dto.ApartmentId,
            BilledToUserId = billedToUserId,
            Period = dto.Period,
            BaseAmount = baseAmount,
            DueDate = dto.DueDate,
            Status = BillingStatus.Unpaid,
            IsVacantRate = isVacantRate
        };

        await _uow.Bills.AddAsync(bill);
        await _uow.SaveChangesAsync();

        await _notificationService.CreateAsync(
            billedToUserId,
            "New Maintenance Bill",
            $"A maintenance bill of ₹{baseAmount} for {dto.Period} is due by {dto.DueDate:dd MMM yyyy}.{(isVacantRate ? " (Vacant rate applied)" : "")}");

        return await MapToDtoAsync(bill);
    }

    public async Task ApplyPenaltiesAsync()
    {
        var allBills = await _uow.Bills.GetAllAsync();
        var overdue = allBills
            .Where(b => (b.Status == BillingStatus.Unpaid || b.Status == BillingStatus.Processing)
                        && b.DueDate < DateTime.UtcNow)
            .ToList();

        foreach (var bill in overdue)
        {
            bill.Penalty = Math.Round(bill.BaseAmount * 0.05m, 2);
            bill.Status = BillingStatus.Overdue;
            bill.UpdatedAt = DateTime.UtcNow;
            await _uow.Bills.UpdateAsync(bill);

            await _notificationService.CreateAsync(
                bill.BilledToUserId,
                "Bill Overdue",
                $"Your maintenance bill for {bill.Period} is overdue. A penalty of ₹{bill.Penalty} has been applied.");
        }

        await _uow.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var bill = await _uow.Bills.GetByIdAsync(id)
            ?? throw new NotFoundException("Bill", id);

        if (bill.Status == BillingStatus.Paid || bill.Status == BillingStatus.Disputed)
            throw new BadRequestException("Cannot delete a paid or disputed bill.");

        await _uow.Bills.DeleteAsync(bill);
        await _uow.SaveChangesAsync();
    }

    internal async Task<BillResponseDto> MapToDtoAsync(Bill b)
    {
        var apartment = await _uow.Apartments.GetByIdAsync(b.ApartmentId);
        var billedToUser = await _uow.Users.GetByIdAsync(b.BilledToUserId);
        return new BillResponseDto
        {
            Id = b.Id,
            ApartmentId = b.ApartmentId,
            ApartmentBlock = apartment?.Block ?? "",
            ApartmentNumber = apartment?.Number ?? "",
            BilledToUserId = b.BilledToUserId,
            BilledToUserName = billedToUser?.Name ?? "",
            Period = b.Period,
            BaseAmount = b.BaseAmount,
            Penalty = b.Penalty,
            Total = b.Total,
            DueDate = b.DueDate,
            PaidAt = b.PaidAt,
            Status = b.Status,
            TransactionRef = b.TransactionRef,
            IsVacantRate = b.IsVacantRate
        };
    }

    private async Task<IEnumerable<BillResponseDto>> MapToDtoListAsync(IEnumerable<Bill> bills)
    {
        var result = new List<BillResponseDto>();
        foreach (var b in bills)
            result.Add(await MapToDtoAsync(b));
        return result;
    }
}
