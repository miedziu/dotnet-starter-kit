using Finbuckle.MultiTenant.Abstractions;
using FSH.Framework.Quota;
using FSH.Framework.Shared.Quota;
using FSH.Modules.Billing.Data;
using FSH.Modules.Billing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FSH.Modules.Billing.Services;

public sealed class UsageReporter : IUsageReporter
{
    private readonly BillingDbContext _db;
    private readonly IQuotaService _quotas;
    private readonly QuotaPlanResolver _planResolver;
    private readonly ILogger<UsageReporter> _logger;

    public UsageReporter(
        BillingDbContext db,
        IQuotaService quotas,
        QuotaPlanResolver planResolver,
        ILogger<UsageReporter> logger)
    {
        _db = db;
        _quotas = quotas;
        _planResolver = planResolver;
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
        foreach (var resource in Enum.GetValues<QuotaResource>())
        {
            var already = existing.FirstOrDefault(s => s.Resource == resource);
            if (already is not null)
            {
                snapshots.Add(already);
                continue;
            }

            var used = await _quotas.GetCurrentAsync(resource, cancellationToken).ConfigureAwait(false);
            var limit = _planResolver.ResolveLimit(resource);
            var snap = UsageSnapshot.Capture(periodYear, periodMonth, resource, used, limit);
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