using FSH.Modules.Auditing.Contracts.v1.Dtos;
using Mediator;

namespace FSH.Modules.Auditing.Contracts.v1;

public sealed class GetSecurityAuditsQuery : IQuery<IReadOnlyList<AuditSummaryDto>>
{
    public SecurityAction? Action { get; init; }

    public string? UserId { get; init; }

    public DateTime? FromUtc { get; init; }

    public DateTime? ToUtc { get; init; }
}