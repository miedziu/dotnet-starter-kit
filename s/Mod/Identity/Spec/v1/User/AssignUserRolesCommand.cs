using FSH.Modules.Identity.Contracts.v1.Dtos;
using Mediator;

namespace FSH.Modules.Identity.Contracts.v1.Users;

public sealed class AssignUserRolesCommand : ICommand<string>
{
    public required string UserId { get; init; }
    public List<UserRoleDto> UserRoles { get; init; } = new();
}
