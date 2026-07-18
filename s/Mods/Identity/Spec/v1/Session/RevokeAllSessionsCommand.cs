using Mediator;

namespace FSH.Mods.Identity.Spec.v1.Session;

public sealed record RevokeAllSessionsCommand(Guid? ExceptSessionId = null) : ICommand<int>;