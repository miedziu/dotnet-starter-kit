using Mediator;

namespace FSH.Mod.Billing.Spec.v1.Usage;

/// <summary>
/// Ops command that captures one usage snapshot for a period.
/// Wraps <c>IUsageReporter.CaptureForPeriodAsync</c>. Idempotent: re-running for the same
/// (period) returns the existing snapshots without creating duplicates.
/// </summary>
public sealed record CaptureUsageSnapshotsCommand(
    int PeriodYear,
    int PeriodMonth) : ICommand<IReadOnlyList<UsageSnapshotDto>>;