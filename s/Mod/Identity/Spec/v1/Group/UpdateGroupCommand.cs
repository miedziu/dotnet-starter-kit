using FSH.Modules.Identity.Contracts.v1.Dtos;
using Mediator;

namespace FSH.Modules.Identity.Contracts.v1.Groups;

public sealed record UpdateGroupCommand(
    Guid Id,
    string Name,
    string? Description,
    bool IsDefault,
    IReadOnlyList<string>? RoleIds) : ICommand<GroupDto>;