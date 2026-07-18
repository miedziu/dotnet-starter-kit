using Mediator;

namespace FSH.Mod.Identity.Spec.v1.Session;

public sealed record AdminRevokeSessionCommand(Guid UserId, Guid SessionId, string? Reason = null) : ICommand<bool>;