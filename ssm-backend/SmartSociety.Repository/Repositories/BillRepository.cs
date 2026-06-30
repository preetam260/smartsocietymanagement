using SmartSociety.Repository.Context;
using SmartSociety.Repository.Interfaces;
using SmartSociety.Domain.Models;
using SmartSociety.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace SmartSociety.Repository.Repositories;

public class BillRepository : Repository<Bill>, IBillRepository
{
    public BillRepository(SmartSocietyDbContext context) : base(context) {}

    public async Task<IEnumerable<Bill>> GetPendingBillsAsync()
    {
        return await _context.Bills
                       .Where(mb => mb.Status == BillingStatus.Unpaid
                       || mb.Status == BillingStatus.Overdue)
                       .ToListAsync();
    }

    public async Task<IEnumerable<Bill>> GetByApartmentIdAsync(Guid id)
    {
        return await _context.Bills
                       .Where(b => b.ApartmentId == id)
                       .OrderByDescending(b => b.DueDate)
                       .ToListAsync();
    }
    public async Task<IEnumerable<Bill>> GetByPeriodAsync(string period)
    {
        return await _context.Bills
                       .Where(b => b.Period == period)
                       .ToListAsync();
    }

    public async Task<Bill?> GetByApartmentAndPeriodAsync(Guid apartmentId, string period)
    {
        return await _context.Bills
                       .FirstOrDefaultAsync(b => b.ApartmentId == apartmentId
                              && b.Period == period);
    }
}
