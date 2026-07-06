using Mediator;

namespace FSH.Modules.Identity.Contracts.v1.Impersonation.GetImpersonationGrants;

// Status (null = all) and ActorUserId filters.
public sealed record GetImpersonationGrantsQuery(
    ImpersonationGrantStatus? Status = null,
    string? ActorUserId = null,
    int Take = 100)
    : IQuery<IReadOnlyList<ImpersonationGrantDto>>;
