using SmartSociety.Domain.Enums;

namespace SmartSociety.Application.DTOs;

public class BillResponseDto
{
    public Guid Id {get; set;}
    public Guid ApartmentId {get; set;}
    public string ApartmentBlock {get; set;} = string.Empty;
    public string ApartmentNumber {get; set;} = string.Empty;
    public Guid BilledToUserId {get; set;}
    public string BilledToUserName {get; set;} = string.Empty;
    public string Period {get; set;} = string.Empty;
    public decimal BaseAmount {get; set;}
    public decimal Penalty {get; set;}
    public decimal Total {get; set;}
    public DateTime DueDate {get; set;}
    public DateTime? PaidAt {get; set;}
    public BillingStatus Status {get; set;}
    public string? TransactionRef {get; set;}
    public bool IsVacantRate {get; set;}
}
