using Mediator;

namespace FSH.Modules.Identity.Contracts.v1.Sessions;

public sealed record RevokeSessionCommand(Guid SessionId) : ICommand<bool>;