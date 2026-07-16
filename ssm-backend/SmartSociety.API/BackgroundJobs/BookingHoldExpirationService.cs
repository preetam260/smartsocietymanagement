using System.Diagnostics;
using SmartSociety.Application.Interfaces;

namespace SmartSociety.API.BackgroundJobs;

public class BookingHoldExpirationService : BackgroundService
{
    private static readonly TimeSpan CheckInterval =
        TimeSpan.FromMinutes(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingHoldExpirationService> _logger;

    public BookingHoldExpirationService(IServiceScopeFactory scopeFactory, ILogger<BookingHoldExpirationService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Booking hold expiration background service started. " +
            "Check interval: {CheckInterval}",
            CheckInterval);

        await RunExpirationCycleAsync(stoppingToken);

        using var timer = new PeriodicTimer(CheckInterval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await RunExpirationCycleAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
        }

        _logger.LogInformation("Booking hold expiration background service stopped.");
    }

    private async Task RunExpirationCycleAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogDebug("Booking hold expiration cycle started at {StartedAtUtc}", DateTime.UtcNow);

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

            await bookingService.ExpireHoldsAsync();

            stopwatch.Stop();

            _logger.LogInformation("Booking hold expiration cycle completed successfully " + "in {ElapsedMilliseconds} ms",
                stopwatch.ElapsedMilliseconds);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Booking hold expiration cycle was cancelled " +
                "because the application is shutting down.");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex, "Booking hold expiration cycle failed after " +
                "{ElapsedMilliseconds} ms", stopwatch.ElapsedMilliseconds);
        }
    }
}