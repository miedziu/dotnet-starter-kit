using FSH.Modules.Auditing.Contracts.v1.Dtos;
using Mediator;

namespace FSH.Modules.Auditing.Contracts.v1;

public sealed record GetAuditByIdQuery(Guid Id) : IQuery<AuditDetailDto>;