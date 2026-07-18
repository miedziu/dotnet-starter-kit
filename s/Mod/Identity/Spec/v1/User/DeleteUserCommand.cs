using Mediator;

namespace FSH.Mod.Identity.Spec.v1.User;

public sealed record DeleteUserCommand(string Id) : ICommand<Unit>;