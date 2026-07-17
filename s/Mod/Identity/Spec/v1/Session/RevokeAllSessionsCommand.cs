using Mediator;

namespace FSH.Modules.Identity.Contracts.v1.Sessions;

public sealed record RevokeAllSessionsCommand(Guid? ExceptSessionId = null) : ICommand<int>;