using Mediator;

namespace FSH.Mods.Identity.Spec.v1.User;

public sealed record DeleteUserCommand(string Id) : ICommand<Unit>;