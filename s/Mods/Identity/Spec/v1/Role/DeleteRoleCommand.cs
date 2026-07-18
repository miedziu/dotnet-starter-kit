using Mediator;

namespace FSH.Mods.Identity.Spec.v1.Role;

public sealed record DeleteRoleCommand(string Id) : ICommand<Unit>;