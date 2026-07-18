namespace FSH.Mod.Identity.Spec.Services;

/// <summary>
/// Service for user permission operations.
/// </summary>
public interface IUserPermissionService
{
    /// <summary>
    /// Gets all permissions for a user.
    /// </summary>
    Task<List<string>?> GetPermissionsAsync(string userId, CancellationToken ct);

    /// <summary>
    /// Checks if a user has a specific permission.
    /// </summary>
    Task<bool> HasPermissionAsync(string userId, string permission, CancellationToken ct = default);

    /// <summary>
    /// Invalidates the permission cache for a user.
    /// </summary>
    Task InvalidatePermissionCacheAsync(string userId, CancellationToken ct);
}