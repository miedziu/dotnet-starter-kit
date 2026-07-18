using Mediator;

namespace FSH.Mods.Identity.Spec.v1.User;

public sealed record ConfirmEmailCommand(string UserId, string Code) : ICommand<string>;