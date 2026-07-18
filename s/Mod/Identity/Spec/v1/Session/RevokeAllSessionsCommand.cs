using Mediator;

namespace FSH.Mod.Identity.Spec.v1.Session;

public sealed record RevokeAllSessionsCommand(Guid? ExceptSessionId = null) : ICommand<int>;