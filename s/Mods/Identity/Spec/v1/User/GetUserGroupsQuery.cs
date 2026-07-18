using Mediator;

namespace FSH.Mods.Identity.Spec.v1.User;

public sealed record GetUserGroupsQuery(string UserId) : IQuery<IEnumerable<GroupDto>>;