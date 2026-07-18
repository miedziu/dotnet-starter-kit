using Mediator;

namespace FSH.Mod.Audit.Spec.v1.Audit;

public sealed class GetAuditSummaryQuery : IQuery<AuditSummaryAggregateDto>
{
    public DateTime? FromUtc { get; init; }

    public DateTime? ToUtc { get; init; }
}