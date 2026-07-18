using Mediator;

namespace FSH.Mods.Identity.Spec.v1.Role;

public sealed record GetRoleQuery(string Id) : IQuery<RoleDto?>;