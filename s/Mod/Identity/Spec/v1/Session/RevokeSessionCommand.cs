using Mediator;

namespace FSH.Mod.Identity.Spec.v1.Session;

public sealed record RevokeSessionCommand(Guid SessionId) : ICommand<bool>;