using SmartSociety.Repository.Context;
using SmartSociety.Repository.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace SmartSociety.Repository.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly SmartSocietyDbContext _context;

    public IUserRepository Users { get; }
    public IResidentRepository Residents { get; }
    public IApartmentRepository Apartments { get; }
    public IVisitorRepository Visitors { get; }
    public IVisitorEntryRepository VisitorEntries { get; }
    public IBookingRepository Bookings { get; }
    public IBillRepository Bills { get; }
    public IFacilityRepository Facilities { get; }
    public IAnnouncementRepository Announcements { get; }
    public INotificationRepository Notifications { get; }
    public IComplaintRepository Complaints { get; }

    public UnitOfWork(
        SmartSocietyDbContext context,
        IUserRepository users,
        IResidentRepository residents,
        IApartmentRepository apartments,
        IVisitorRepository visitors,
        IVisitorEntryRepository visitorEntries,
        IBookingRepository bookings,
        IBillRepository bills,
        IFacilityRepository facilities,
        IAnnouncementRepository announcements,
        INotificationRepository notifications,
        IComplaintRepository complaints)
    {
        _context = context;
        Users = users;
        Residents = residents;
        Apartments = apartments;
        Visitors = visitors;
        VisitorEntries = visitorEntries;
        Bookings = bookings;
        Bills = bills;
        Facilities = facilities;
        Announcements = announcements;
        Notifications = notifications;
        Complaints = complaints;
    }
    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();

    public async Task<IDbContextTransaction> BeginTransactionAsync()
        => await _context.Database.BeginTransactionAsync();

    public void Dispose()
        => _context.Dispose();
}