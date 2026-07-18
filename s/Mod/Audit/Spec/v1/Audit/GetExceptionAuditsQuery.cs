using Mediator;

namespace FSH.Mod.Audit.Spec.v1.Audit;

public sealed class GetExceptionAuditsQuery : IQuery<IReadOnlyList<AuditSummaryDto>>
{
    public ExceptionArea? Area { get; init; }

    public AuditSeverity? Severity { get; init; }

    public string? ExceptionType { get; init; }

    public string? RouteOrLocation { get; init; }

    public DateTime? FromUtc { get; init; }

    public DateTime? ToUtc { get; init; }
}