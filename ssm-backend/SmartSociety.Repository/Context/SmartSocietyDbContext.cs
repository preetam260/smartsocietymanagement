using Microsoft.EntityFrameworkCore;
using SmartSociety.Domain.Models;

namespace SmartSociety.Repository.Context;

public class SmartSocietyDbContext : DbContext
{
    // Constructor receives DB options from Program.cs
    public SmartSocietyDbContext(DbContextOptions<SmartSocietyDbContext> options)
        : base(options)
    {
    }

    // Represents Users table
    public DbSet<User> Users { get; set; }

    // Represents Residents table
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

        // USER CONFIG

        modelBuilder.Entity<User>()
            .HasIndex(x => x.Email)
            .IsUnique();

        // APARTMENT RELATIONSHIPS

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

        // VISITOR RELATIONSHIPS

        modelBuilder.Entity<Visitor>()
            .HasOne(x => x.Apartment)
            .WithMany(x => x.Visitors)
            .HasForeignKey(x => x.ApartmentId);

        // VISITOR ENTRY RELATIONSHIPS

        modelBuilder.Entity<VisitorEntry>()
            .HasOne(x => x.Visitor)
            .WithMany(x => x.Entries)
            .HasForeignKey(x => x.VisitorId);

        // BOOKING RELATIONSHIPS

        modelBuilder.Entity<Booking>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId);

        modelBuilder.Entity<Booking>()
            .HasOne(x => x.Facility)
            .WithMany(x => x.Bookings)
            .HasForeignKey(x => x.FacilityId);

        // BILL RELATIONSHIPS

        modelBuilder.Entity<Bill>()
            .HasOne(x => x.Apartment)
            .WithMany(x => x.Bills)
            .HasForeignKey(x => x.ApartmentId);

        modelBuilder.Entity<Bill>()
            .HasOne(x => x.BilledToUser)
            .WithMany()
            .HasForeignKey(x => x.BilledToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ignore computed property
        modelBuilder.Entity<Bill>()
            .Ignore(x => x.Total);

        // COMPLAINT RELATIONSHIPS

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