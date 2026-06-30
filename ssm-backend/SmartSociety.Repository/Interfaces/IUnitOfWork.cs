using Microsoft.EntityFrameworkCore.Storage;

namespace SmartSociety.Repository.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IResidentRepository Residents { get; }
    IApartmentRepository Apartments { get; }
    IVisitorRepository Visitors { get; }
    IVisitorEntryRepository VisitorEntries { get; }
    IBookingRepository Bookings { get; }
    IBillRepository Bills { get; }
    IFacilityRepository Facilities { get; }
    IAnnouncementRepository Announcements { get; }
    INotificationRepository Notifications { get; }
    IComplaintRepository Complaints { get; }

    Task<int> SaveChangesAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();
}