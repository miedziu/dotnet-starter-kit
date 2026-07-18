using Mediator;

namespace FSH.Mods.Identity.Spec.v1.Session;

public sealed record RevokeSessionCommand(Guid SessionId) : ICommand<bool>;