namespace SmartSociety.Application.Interfaces;

public interface IComplaintTriageQueue
{
    void Enqueue(Guid complaintId);
    IAsyncEnumerable<Guid> DequeueAllAsync(CancellationToken ct);
}