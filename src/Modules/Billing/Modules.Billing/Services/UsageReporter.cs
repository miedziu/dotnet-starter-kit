using FSH.Modules.Billing.Data;
using FSH.Modules.Billing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FSH.Modules.Billing.Services;

public sealed class UsageReporter : IUsageReporter
{
    private readonly BillingDbContext _db;
    private readonly ILogger<UsageReporter> _logger;

    public UsageReporter(
        BillingDbContext db,
        ILogger<UsageReporter> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<UsageSnapshot>> CaptureForPeriodAsync(
        int periodYear,
        int periodMonth,
        CancellationToken cancellationToken = default)
    {
        var existing = await _db.UsageSnapshots
            .Where(s => s.PeriodYear == periodYear && s.PeriodMonth == periodMonth)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var snapshots = new List<UsageSnapshot>(capacity: 4);

        var already = existing.FirstOrDefault();
        if (already is not null)
        {
            snapshots.Add(already);
        }
        else
        {
            var used = 0; //
            var limit = long.MaxValue; //

            var snap = UsageSnapshot.Capture(periodYear, periodMonth, used, limit);
            _db.UsageSnapshots.Add(snap);
            snapshots.Add(snap);
        }


        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("[Billing] captured {Count} usage snapshots for period {Year}-{Month:00}",
                snapshots.Count, periodYear, periodMonth);
        }
        return snapshots;
    }
}