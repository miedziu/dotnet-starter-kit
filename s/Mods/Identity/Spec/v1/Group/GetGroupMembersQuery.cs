using Mediator;

namespace FSH.Mods.Identity.Spec.v1.Group;

public sealed record GetGroupMembersQuery(Guid GroupId) : IQuery<IEnumerable<GroupMemberDto>>;