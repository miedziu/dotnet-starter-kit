using FSH.Modules.Auditing.Contracts.v1.Dtos;
using Mediator;

namespace FSH.Modules.Auditing.Contracts.v1;

public sealed class GetAuditSummaryQuery : IQuery<AuditSummaryAggregateDto>
{
    public DateTime? FromUtc { get; init; }

    public DateTime? ToUtc { get; init; }
}