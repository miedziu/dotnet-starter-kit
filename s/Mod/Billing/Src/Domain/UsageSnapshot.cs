using FSH.Framework.Core.Domain;

namespace FSH.Mod.Billing.Domain;

/// <summary>
/// Frozen record of usage for a single resource in a billing period.
/// </summary>
public sealed class UsageSnapshot : BaseEntity<Guid>
{
    public int PeriodYear { get; private set; }
    public int PeriodMonth { get; private set; }
    public long UsedUnits { get; private set; }
    public long LimitUnits { get; private set; }
    public DateTime CapturedAtUtc { get; private set; }

    private UsageSnapshot() { }

    public static UsageSnapshot Capture(
        int periodYear,
        int periodMonth,
        long usedUnits,
        long limitUnits)
    {
        if (periodYear is < 2000 or > 2100)
        {
            throw new ArgumentOutOfRangeException(nameof(periodYear));
        }
        if (periodMonth is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(periodMonth));
        }

        return new UsageSnapshot
        {
            Id = Guid.CreateVersion7(),
            PeriodYear = periodYear,
            PeriodMonth = periodMonth,
            UsedUnits = usedUnits,
            LimitUnits = limitUnits,
            CapturedAtUtc = DateTime.UtcNow
        };
    }

    public long Overage => UsedUnits > LimitUnits ? UsedUnits - LimitUnits : 0;
}