using Mediator;

namespace FSH.Mod.Audit.Spec.v1.Audit;

public sealed record GetAuditByIdQuery(Guid Id) : IQuery<AuditDetailDto>;