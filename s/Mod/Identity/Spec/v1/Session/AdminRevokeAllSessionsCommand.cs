using Mediator;

namespace FSH.Mod.Identity.Spec.v1.Session;

public sealed record AdminRevokeAllSessionsCommand(Guid UserId, string? Reason = null) : ICommand<int>;