using FSH.Modules.Auditing.Contracts.v1.Dtos;
using Mediator;

namespace FSH.Modules.Auditing.Contracts.v1;

public sealed class GetAuditsByCorrelationQuery : IQuery<IReadOnlyList<AuditSummaryDto>>
{
    public string CorrelationId { get; init; } = default!;

    public DateTime? FromUtc { get; init; }

    public DateTime? ToUtc { get; init; }
}