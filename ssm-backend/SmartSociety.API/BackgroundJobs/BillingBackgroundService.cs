using Microsoft.EntityFrameworkCore;
using SmartSociety.Application.Interfaces;
using SmartSociety.Domain.Enums;
using SmartSociety.Domain.Models;
using SmartSociety.Repository.Context;

namespace SmartSociety.API.BackgroundJobs;

public class BillingBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _config;
    private readonly ILogger<BillingBackgroundService> _logger;

    public BillingBackgroundService(
        IServiceProvider serviceProvider,
        IConfiguration config,
        ILogger<BillingBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BillingBackgroundService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;

            var nextMidnight = now.Date.AddDays(1);
            var delay = nextMidnight - now;

            _logger.LogInformation("BillingBackgroundService: next run at {NextRun} (in {Delay})", nextMidnight, delay);
            await Task.Delay(delay, stoppingToken);

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<SmartSocietyDbContext>();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                var today = DateTime.UtcNow;

                if (today.Day == 1)
                {
                    await GenerateMonthlyBillsAsync(context, notificationService, today);
                }
                await MarkOverdueBillsAsync(context, notificationService, today);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BillingBackgroundService encountered an error.");
            }
        }
    }

    private async Task GenerateMonthlyBillsAsync(SmartSocietyDbContext context, INotificationService notificationService, DateTime today)
    {
        var billingSettings = _config.GetSection("BillingSettings");
        var defaultBaseAmount = decimal.Parse(billingSettings["DefaultBaseAmount"] ?? "5000");
        var vacantRateMultiplier = decimal.Parse(billingSettings["VacantRateMultiplier"] ?? "0.25");
        var dueDay = int.Parse(billingSettings["DueDay"] ?? "15");

        var period = today.ToString("MM-yyyy");
        var dueDate = new DateTime(today.Year, today.Month, dueDay, 0, 0, 0, DateTimeKind.Utc);

        var apartments = await context.Apartments.ToListAsync();

        foreach (var apartment in apartments)
        {
            var existingBill = await context.Bills
                .FirstOrDefaultAsync(b => b.ApartmentId == apartment.Id && b.Period == period);
            if (existingBill != null)
                continue;

            var currentResident = await context.Residents
                .FirstOrDefaultAsync(r => r.ApartmentId == apartment.Id && r.MoveOutDate == null);

            Guid billedToUserId;
            decimal baseAmount;
            bool isVacantRate;

            if (currentResident != null)
            {
                billedToUserId = currentResident.UserId;
                baseAmount = defaultBaseAmount;
                isVacantRate = false;
            }
            else
            {
                billedToUserId = apartment.OwnerId;
                baseAmount = Math.Round(defaultBaseAmount * vacantRateMultiplier, 2);
                isVacantRate = true;
            }

            var bill = new Bill
            {
                ApartmentId = apartment.Id,
                BilledToUserId = billedToUserId,
                Period = period,
                BaseAmount = baseAmount,
                DueDate = dueDate,
                Status = BillingStatus.Unpaid,
                IsVacantRate = isVacantRate
            };

            context.Bills.Add(bill);
            await context.SaveChangesAsync();

            _logger.LogInformation(
                "Generated bill for apartment {Block}-{Number}, period {Period}, amount ₹{Amount}{Vacant}",
                apartment.Block, apartment.Number, period, baseAmount,
                isVacantRate ? " (vacant rate)" : "");

            await notificationService.CreateAsync(
                billedToUserId,
                "New Maintenance Bill",
                $"A maintenance bill of ₹{baseAmount} for {period} is due by {dueDate:dd MMM yyyy}.{(isVacantRate ? " (Vacant rate applied)" : "")}");
        }
    }

    private static int CalculateMonthsOverdue(DateTime dueDate, DateTime currentDate)
    {
        if (currentDate <= dueDate)
        {
            return 0;
        }

        var months =
            (currentDate.Year - dueDate.Year) * 12 +
            currentDate.Month -
            dueDate.Month;

        return Math.Max(1, months);
    }
    private async Task MarkOverdueBillsAsync(SmartSocietyDbContext context, INotificationService notificationService, DateTime today)
    {
        var billingSettings =
            _config.GetSection("BillingSettings");

        var penaltyRate =
            decimal.Parse(
                billingSettings["PenaltyRate"] ?? "0.05");

        var overdueBills = await context.Bills
            .Where(b =>
                (
                    b.Status == BillingStatus.Unpaid ||
                    b.Status == BillingStatus.Processing ||
                    b.Status == BillingStatus.Overdue
                )
                && b.DueDate < today)
            .ToListAsync();

        foreach (var bill in overdueBills)
        {
            var wasAlreadyOverdue =
                bill.Status == BillingStatus.Overdue;

            var monthsOverdue = CalculateMonthsOverdue(
                    bill.DueDate,
                    today);

            var newPenalty = Math.Round(
                    bill.BaseAmount *
                    penaltyRate *
                    monthsOverdue,
                    2);

            var penaltyChanged =
                bill.Penalty != newPenalty;

            bill.Penalty = newPenalty;
            bill.Status = BillingStatus.Overdue;
            bill.UpdatedAt = DateTime.UtcNow;

            _logger.LogInformation(
                "Bill {BillId} for period {Period} is {MonthsOverdue} month(s) overdue. Penalty: ₹{Penalty}", bill.Id, bill.Period, monthsOverdue, bill.Penalty
                );

            if (!wasAlreadyOverdue || penaltyChanged)
            {
                await notificationService.CreateAsync(
                    bill.BilledToUserId,
                    "Bill Overdue",
                    $"Your maintenance bill for {bill.Period} " +
                    $"is {monthsOverdue} month(s) overdue. " +
                    $"The current penalty is ₹{bill.Penalty}.");
            }
        }

        await context.SaveChangesAsync();
    }
}
