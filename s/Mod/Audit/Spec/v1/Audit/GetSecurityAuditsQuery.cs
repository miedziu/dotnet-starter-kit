using Mediator;

namespace FSH.Mod.Audit.Spec.v1.Audit;

public sealed class GetSecurityAuditsQuery : IQuery<IReadOnlyList<AuditSummaryDto>>
{
    public SecurityAction? Action { get; init; }

    public string? UserId { get; init; }

    public DateTime? FromUtc { get; init; }

    public DateTime? ToUtc { get; init; }
}