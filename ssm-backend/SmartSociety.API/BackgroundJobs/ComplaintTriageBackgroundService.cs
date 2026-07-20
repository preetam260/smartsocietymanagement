using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartSociety.Application.Complaints.Triage;
using SmartSociety.Application.Interfaces;
using SmartSociety.Domain.Enums;
using SmartSociety.Repository.Context;


namespace SmartSociety.Infrastructure.BackgroundJobs;

public class ComplaintTriageBackgroundService : BackgroundService
{
    private readonly IComplaintTriageQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ComplaintTriageBackgroundService> _logger;

    public ComplaintTriageBackgroundService(
        IComplaintTriageQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<ComplaintTriageBackgroundService> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var complaintId in _queue.DequeueAllAsync(stoppingToken))
        {
            try
            {
                await ProcessAsync(complaintId, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed processing triage for complaint {ComplaintId}", complaintId);
            }
        }
    }

    private async Task ProcessAsync(Guid complaintId, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SmartSocietyDbContext>();
        var triageService = scope.ServiceProvider.GetRequiredService<IComplaintTriageService>();

        var complaint = await db.Complaints.FirstOrDefaultAsync(c => c.Id == complaintId, ct);
        if (complaint is null) return;

        var openComplaintsSameBlock = await db.Complaints
            .Where(c => c.Id != complaintId
                        && c.ApartmentId == complaint.ApartmentId
                        && c.Status == ComplaintStatus.Open
                        && c.CreatedAt >= DateTime.UtcNow.AddDays(-3))
            .ToListAsync(ct);

        var result = await triageService.TriageAsync(complaint, openComplaintsSameBlock, ct);
        if (result is null)
        {
            complaint.TriageProcessed = true;
            await db.SaveChangesAsync(ct);
            return;
        }

        complaint.Category = result.Category;
        complaint.Priority = result.Priority;
        complaint.DraftAdminResponse = result.DraftResponse;
        complaint.PossibleDuplicateIdsCsv = result.PossibleDuplicateIds.Count > 0
            ? string.Join(',', result.PossibleDuplicateIds)
            : null;
        complaint.TriageProcessed = true;

        await db.SaveChangesAsync(ct);
    }
}