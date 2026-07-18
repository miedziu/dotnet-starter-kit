using Mediator;

namespace FSH.Mod.Identity.Spec.v1.User;

public sealed record GetUserGroupsQuery(string UserId) : IQuery<IEnumerable<GroupDto>>;