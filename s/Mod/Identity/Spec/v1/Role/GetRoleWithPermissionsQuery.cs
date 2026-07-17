using FSH.Modules.Identity.Contracts.v1.Dtos;
using Mediator;

namespace FSH.Modules.Identity.Contracts.v1.Roles;

public sealed record GetRoleWithPermissionsQuery(string Id) : IQuery<RoleDto>;