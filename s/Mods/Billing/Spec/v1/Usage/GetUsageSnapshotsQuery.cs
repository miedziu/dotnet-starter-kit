using Mediator;

namespace FSH.Mods.Billing.Spec.v1.Usage;

public sealed record GetUsageSnapshotsQuery(
    int? PeriodYear = null,
    int? PeriodMonth = null) : IQuery<IReadOnlyList<UsageSnapshotDto>>;