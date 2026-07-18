using FSH.Mod.Billing.Domain;

namespace FSH.Mod.Billing.Services;

/// <summary>
/// Snapshots per-tenant usage for a billing period by reading from <c>IQuotaService</c> and resolving
/// the effective limit. Snapshots are persisted to the <c>UsageSnapshots</c> table so the invoicing
/// math is reproducible even if the plan changes afterwards.
/// </summary>
public interface IUsageReporter
{
    /// <summary>
    /// Captures one <see cref="UsageSnapshot"/> for the given period.
    /// Idempotent: if a snapshot already exists for (period, resource) the existing record is
    /// returned instead of a new one.
    /// </summary>
    Task<IReadOnlyList<UsageSnapshot>> CaptureForPeriodAsync(
        int periodYear,
        int periodMonth,
        CancellationToken ct = default);
}