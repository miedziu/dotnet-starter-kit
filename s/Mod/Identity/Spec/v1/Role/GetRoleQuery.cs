using Mediator;

namespace FSH.Mod.Identity.Spec.v1.Role;

public sealed record GetRoleQuery(string Id) : IQuery<RoleDto?>;