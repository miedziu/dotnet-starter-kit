namespace FSH.Mod.Identity.Spec.Services;

/// <summary>
/// Service for user role management.
/// </summary>
public interface IUserRoleService
{
    /// <summary>
    /// Assigns roles to a user.
    /// </summary>
    Task<string> AssignRolesAsync(string userId, List<UserRoleDto> userRoles, CancellationToken ct);

    /// <summary>
    /// Gets all roles for a user.
    /// </summary>
    Task<List<UserRoleDto>> GetUserRolesAsync(string userId, CancellationToken ct);
}