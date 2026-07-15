using Mediator;

namespace FSH.Modules.Identity.Contracts.v1.Users;

public sealed record ConfirmEmailCommand(string UserId, string Code) : ICommand<string>;