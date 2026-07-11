namespace FSH.Modules.Billing.Contracts.Dtos;

public sealed record UsageSnapshotDto(
    Guid Id,
    int PeriodYear,
    int PeriodMonth,
    long UsedUnits,
    long LimitUnits,
    long Overage,
    DateTime CapturedAtUtc);