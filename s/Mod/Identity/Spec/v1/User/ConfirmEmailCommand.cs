using Mediator;

namespace FSH.Mod.Identity.Spec.v1.User;

public sealed record ConfirmEmailCommand(string UserId, string Code) : ICommand<string>;