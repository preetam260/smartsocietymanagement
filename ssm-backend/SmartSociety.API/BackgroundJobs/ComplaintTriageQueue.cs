using System.Threading.Channels;
using SmartSociety.Application.Complaints.Triage;
using SmartSociety.Application.Interfaces;

namespace SmartSociety.Infrastructure.BackgroundJobs;

public class ComplaintTriageQueue : IComplaintTriageQueue
{
    private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>();

    public void Enqueue(Guid complaintId) => _channel.Writer.TryWrite(complaintId);

    public IAsyncEnumerable<Guid> DequeueAllAsync(CancellationToken ct) =>
        _channel.Reader.ReadAllAsync(ct);
}