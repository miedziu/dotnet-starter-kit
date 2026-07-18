using Mediator;

namespace FSH.Mods.Audit.Spec.v1.Audit;

public sealed record GetAuditByIdQuery(Guid Id) : IQuery<AuditDetailDto>;