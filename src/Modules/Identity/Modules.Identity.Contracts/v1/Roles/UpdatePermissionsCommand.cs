using Mediator;

namespace FSH.Modules.Identity.Contracts.v1.Roles;

public class UpdatePermissionsCommand : ICommand<string>
{
    /// <summary>
    /// The ID of the role to update.
    /// </summary>
    public string RoleId { get; init; } = default!;

    /// <summary>
    /// The list of permissions to assign to the role.
    /// </summary>
    public List<string> Permissions { get; init; } = [];
}