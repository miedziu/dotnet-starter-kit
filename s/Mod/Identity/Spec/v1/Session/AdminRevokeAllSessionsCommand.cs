using Mediator;

namespace FSH.Modules.Identity.Contracts.v1.Sessions;

public sealed record AdminRevokeAllSessionsCommand(Guid UserId, string? Reason = null) : ICommand<int>;