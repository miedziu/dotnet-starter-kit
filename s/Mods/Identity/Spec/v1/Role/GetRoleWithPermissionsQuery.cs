using Mediator;

namespace FSH.Mods.Identity.Spec.v1.Role;

public sealed record GetRoleWithPermissionsQuery(string Id) : IQuery<RoleDto>;