using Mediator;

namespace FSH.Mod.Identity.Spec.v1.Group;

public sealed record AddUsersToGroupCommand(Guid GroupId, IReadOnlyList<string> UserIds) : ICommand<AddUsersToGroupResponse>;

public sealed record AddUsersToGroupResponse(int AddedCount, List<string> AlreadyMemberUserIds);