using FSH.Modules.Billing.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Billing.Contracts.v1.Usage;

public sealed record GetUsageSnapshotsQuery(
    int? PeriodYear = null,
    int? PeriodMonth = null) : IQuery<IReadOnlyList<UsageSnapshotDto>>;