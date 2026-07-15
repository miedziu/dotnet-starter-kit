using FSH.Modules.Identity.Contracts.v1.Dtos;
using Mediator;

namespace FSH.Modules.Identity.Contracts.v1.Roles;

public sealed record GetRoleQuery(string Id) : IQuery<RoleDto?>;