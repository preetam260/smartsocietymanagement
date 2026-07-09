using SmartSociety.Domain.Enums;

namespace SmartSociety.Domain.Models;
public class Bill : BaseEntity
{
    public Guid ApartmentId {get; set;} 
    public Guid BilledToUserId {get; set;}
    public string Period {get; set;} = string.Empty; 
    public decimal BaseAmount {get; set;}
    public decimal Penalty {get; set;}
    public decimal Total => BaseAmount + Penalty;
    public DateTime DueDate {get; set;}
    public DateTime? PaidAt {get; set;}
    public BillingStatus Status {get; set;} = BillingStatus.Unpaid;
    public string? TransactionRef {get; set;}
    public bool IsVacantRate {get; set;}

    public Apartment? Apartment {get; set;}
    public User? BilledToUser {get; set;}
}
