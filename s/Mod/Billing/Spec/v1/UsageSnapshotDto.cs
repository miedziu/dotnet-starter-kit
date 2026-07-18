namespace FSH.Mod.Billing.Spec.v1;

public sealed record UsageSnapshotDto(
    Guid Id,
    int PeriodYear,
    int PeriodMonth,
    long UsedUnits,
    long LimitUnits,
    long Overage,
    DateTime CapturedAtUtc);