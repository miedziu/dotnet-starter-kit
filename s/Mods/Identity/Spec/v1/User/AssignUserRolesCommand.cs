using Mediator;

namespace FSH.Mods.Identity.Spec.v1.User;

public sealed class AssignUserRolesCommand : ICommand<string>
{
    public required string UserId { get; init; }
    public List<UserRoleDto> UserRoles { get; init; } = new();
}
