using Mediator;

namespace FSH.Mods.Identity.Spec.v1.Impersonation;

public sealed record RevokeImpersonationGrantCommand(
    Guid GrantId,
    string? Reason)
    : ICommand<ImpersonationGrantDto>;