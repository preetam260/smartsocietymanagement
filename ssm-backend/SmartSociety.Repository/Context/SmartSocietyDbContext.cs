using Microsoft.EntityFrameworkCore;
using SmartSociety.Domain.Models;

namespace SmartSociety.Repository.Context;

public class SmartSocietyDbContext : DbContext
{
    public SmartSocietyDbContext(DbContextOptions<SmartSocietyDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Resident> Residents { get; set; }

    public DbSet<Apartment> Apartments { get; set; }

    public DbSet<Visitor> Visitors { get; set; }

    public DbSet<VisitorEntry> VisitorEntries { get; set; }

    public DbSet<Facility> Facilities { get; set; }

    public DbSet<Booking> Bookings { get; set; }

    public DbSet<Bill> Bills { get; set; }

    public DbSet<Announcement> Announcements { get; set; }

    public DbSet<Notification> Notifications { get; set; }

    public DbSet<Complaint> Complaints { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(x => x.Email)
            .IsUnique();

        modelBuilder.Entity<Apartment>()
            .HasOne(x => x.Owner)
            .WithMany()
            .HasForeignKey(x => x.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Resident>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId);

        modelBuilder.Entity<Resident>()
            .HasOne(x => x.Apartment)
            .WithMany(x => x.Residents)
            .HasForeignKey(x => x.ApartmentId);

        modelBuilder.Entity<Visitor>()
            .HasOne(x => x.Apartment)
            .WithMany(x => x.Visitors)
            .HasForeignKey(x => x.ApartmentId);

        modelBuilder.Entity<VisitorEntry>()
            .HasOne(x => x.Visitor)
            .WithMany(x => x.Entries)
            .HasForeignKey(x => x.VisitorId);

        modelBuilder.Entity<Booking>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId);

        modelBuilder.Entity<Booking>()
            .HasOne(x => x.Facility)
            .WithMany(x => x.Bookings)
            .HasForeignKey(x => x.FacilityId);

        modelBuilder.Entity<Booking>()
            .Property(x => x.SeatsBooked)
            .HasDefaultValue(1);

        modelBuilder.Entity<Booking>()
            .HasIndex(x => new { x.FacilityId, x.StartTime, x.EndTime });

        modelBuilder.Entity<Bill>()
            .HasOne(x => x.Apartment)
            .WithMany(x => x.Bills)
            .HasForeignKey(x => x.ApartmentId);

        modelBuilder.Entity<Bill>()
            .HasOne(x => x.BilledToUser)
            .WithMany()
            .HasForeignKey(x => x.BilledToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Bill>()
            .Ignore(x => x.Total);

        modelBuilder.Entity<Complaint>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId);

        modelBuilder.Entity<Complaint>()
            .HasOne(x => x.Apartment)
            .WithMany()
            .HasForeignKey(x => x.ApartmentId);
    }
}
