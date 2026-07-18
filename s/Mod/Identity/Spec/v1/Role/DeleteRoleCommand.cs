using Mediator;

namespace FSH.Mod.Identity.Spec.v1.Role;

public sealed record DeleteRoleCommand(string Id) : ICommand<Unit>;