using Mediator;

namespace FSH.Mods.Identity.Spec.v1.Group;

public sealed record CreateGroupCommand(
    string Name,
    string? Description,
    bool IsDefault,
    List<string>? RoleIds) : ICommand<GroupDto>;