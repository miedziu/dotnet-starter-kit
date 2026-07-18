using Mediator;

namespace FSH.Mod.Identity.Spec.v1.Group;

public sealed record RemoveUserFromGroupCommand(Guid GroupId, string UserId) : ICommand<Unit>;