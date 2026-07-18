using Mediator;

namespace FSH.Mod.Identity.Spec.v1.Role;

public sealed record GetRoleWithPermissionsQuery(string Id) : IQuery<RoleDto>;