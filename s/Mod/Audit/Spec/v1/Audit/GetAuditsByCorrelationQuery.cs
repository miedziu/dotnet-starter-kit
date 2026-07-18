using Mediator;

namespace FSH.Mod.Audit.Spec.v1.Audit;

public sealed class GetAuditsByCorrelationQuery : IQuery<IReadOnlyList<AuditSummaryDto>>
{
    public string CorrelationId { get; init; } = default!;

    public DateTime? FromUtc { get; init; }

    public DateTime? ToUtc { get; init; }
}