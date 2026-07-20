using SmartSociety.Application.DTOs;
using SmartSociety.Domain.Models;

namespace SmartSociety.Application.Complaints.Triage;

public interface IComplaintTriageService{

    Task<ComplaintTriageResult?> TriageAsync(Complaint complaint, IReadOnlyList<Complaint> openComplaintsSameBlock, CancellationToken ct = default);
}