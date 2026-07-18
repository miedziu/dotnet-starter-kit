using System.Security.Claims;

namespace FSH.Mods.Identity.Spec.Services;

/// <summary>
/// Basic user info returned by OAuth flows when creating/linking accounts.
/// </summary>
public record UserInfo(string Id, string? Email, string? FirstName, string? LastName);
public interface IIdentityService
{
    /// <summary>
    /// Validates the provided user credentials and returns a unique subject ID with associated claims.
    /// </summary>
    /// <param name="email">User email or username</param>
    /// <param name="password">User password</param>
    /// <param name="twoFactorCode">Optional two-factor authentication code</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Subject ID and claims, or null if invalid</returns>
    Task<(string Subject, IEnumerable<Claim> Claims)?>
        ValidateCredentialsAsync(string email, string password, string? twoFactorCode = null, CancellationToken ct = default);

    /// <summary>
    /// Validates a refresh token and returns its claims if valid.
    /// </summary>
    Task<(string Subject, IEnumerable<Claim> Claims)?>
        ValidateRefreshTokenAsync(string refreshToken, CancellationToken ct = default);

    /// <summary>
    /// Persists a hashed refresh token for the specified subject.
    /// </summary>
    Task StoreRefreshTokenAsync(string subject, string refreshToken, DateTime expiresAtUtc, CancellationToken ct = default);

    /// <summary>
    /// Builds the claim set for a user located in an arbitrary tenant, bypassing Finbuckle's tenant
    /// query filters. Used for impersonation and end-impersonation flows where the current request's
    /// tenant context differs from the target user's tenant. Returns null if the user is not found.
    /// </summary>
    Task<(string Subject, IEnumerable<Claim> Claims)?>
        BuildClaimsForUserAsync(string userId, CancellationToken ct = default);

    /// <summary>
    /// Finds a user by their email address. Used for OAuth login to link external providers.
    /// Returns basic user info without exposing domain entities to the Spec layer.
    /// </summary>
    Task<UserInfo?> FindByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Gets user claims for the specified user ID. Used for OAuth token generation.
    /// </summary>
    Task<List<Claim>> GetUserClaimsAsync(string userId, CancellationToken ct = default);
}