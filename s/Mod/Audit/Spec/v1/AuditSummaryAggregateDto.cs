namespace FSH.Mod.Audit.Spec.v1;

public sealed class AuditSummaryAggregateDto
{
    public IDictionary<AuditEventType, long> EventsByType { get; init; } =
        new Dictionary<AuditEventType, long>();

    public IDictionary<AuditSeverity, long> EventsBySeverity { get; init; } =
        new Dictionary<AuditSeverity, long>();

    public IDictionary<string, long> EventsBySource { get; init; } =
        new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
}