using Mediator;

namespace FSH.Mod.Audit.Spec.v1.Audit;

public sealed class GetAuditsByTraceQuery : IQuery<IReadOnlyList<AuditSummaryDto>>
{
    public string TraceId { get; init; } = default!;

    public DateTime? FromUtc { get; init; }

    public DateTime? ToUtc { get; init; }
}