using Mediator;

namespace FSH.Mods.Identity.Spec.v1.Impersonation;

// Status (null = all) and ActorUserId filters.
public sealed record GetImpersonationGrantsQuery(
    ImpersonationGrantStatus? Status = null,
    string? ActorUserId = null,
    int Take = 100)
    : IQuery<IReadOnlyList<ImpersonationGrantDto>>;