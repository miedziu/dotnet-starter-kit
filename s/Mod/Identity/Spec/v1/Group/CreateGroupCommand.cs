using FSH.Modules.Identity.Contracts.v1.Dtos;
using Mediator;

namespace FSH.Modules.Identity.Contracts.v1.Groups;

public sealed record CreateGroupCommand(
    string Name,
    string? Description,
    bool IsDefault,
    List<string>? RoleIds) : ICommand<GroupDto>;