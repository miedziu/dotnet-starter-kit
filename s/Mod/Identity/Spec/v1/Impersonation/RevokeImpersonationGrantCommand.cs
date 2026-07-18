using Mediator;

namespace FSH.Mod.Identity.Spec.v1.Impersonation;

public sealed record RevokeImpersonationGrantCommand(
    Guid GrantId,
    string? Reason)
    : ICommand<ImpersonationGrantDto>;