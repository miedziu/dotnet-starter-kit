using Mediator;

namespace FSH.Mods.Identity.Spec.v1.User;

public sealed record GetUserRolesQuery(string UserId) : IQuery<List<UserRoleDto>>;