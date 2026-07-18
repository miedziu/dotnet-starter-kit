using Mediator;

namespace FSH.Mods.Identity.Spec.v1.Group;

public sealed record UpdateGroupCommand(
    Guid Id,
    string Name,
    string? Description,
    bool IsDefault,
    IReadOnlyList<string>? RoleIds) : ICommand<GroupDto>;