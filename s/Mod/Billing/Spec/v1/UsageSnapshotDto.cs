namespace FSH.Modules.Billing.Contracts.v1.Dtos;

public sealed record UsageSnapshotDto(
    Guid Id,
    int PeriodYear,
    int PeriodMonth,
    long UsedUnits,
    long LimitUnits,
    long Overage,
    DateTime CapturedAtUtc);