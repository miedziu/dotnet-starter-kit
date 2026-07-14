using FSH.Modules.Identity.Contracts.DTOs;

namespace FSH.Modules.Identity.Contracts.Services;

public interface ISessionService
{
    Task<UserSessionDto> CreateSessionAsync(
        string userId,
        string refreshTokenHash,
        string ipAddress,
        string userAgent,
        DateTime expiresAt,
        CancellationToken ct = default);

    Task<List<UserSessionDto>> GetUserSessionsAsync(
        string userId,
        CancellationToken ct = default);

    Task<List<UserSessionDto>> GetUserSessionsForAdminAsync(
        string userId,
        CancellationToken ct = default);

    /// <summary>
    /// Returns all sessions for admin views.
    /// </summary>
    /// <param name="includeInactive">When true, also returns expired/revoked sessions.</param>
    /// <param name="search">Optional substring filter applied to user name, email, or IP address.</param>
    /// <param name="skip">Pagination offset.</param>
    /// <param name="take">Pagination size (capped server-side).</param>
    /// <param name="ct">Cancellation token.</param>
    Task<(List<UserSessionDto> Items, long TotalCount)> GetAllSessionsAsync(
        bool includeInactive,
        string? search,
        int skip,
        int take,
        CancellationToken ct = default);

    Task<UserSessionDto?> GetSessionAsync(
        Guid sessionId,
        CancellationToken ct = default);

    Task<bool> RevokeSessionAsync(
        Guid sessionId,
        string revokedBy,
        string? reason = null,
        CancellationToken ct = default);

    Task<int> RevokeAllSessionsAsync(
        string userId,
        string revokedBy,
        Guid? exceptSessionId = null,
        string? reason = null,
        CancellationToken ct = default);

    Task<int> RevokeAllSessionsForAdminAsync(
        string userId,
        string revokedBy,
        string? reason = null,
        CancellationToken ct = default);

    Task<bool> RevokeSessionForAdminAsync(
        Guid sessionId,
        string revokedBy,
        string? reason = null,
        CancellationToken ct = default);

    Task UpdateSessionActivityAsync(
        string refreshTokenHash,
        CancellationToken ct = default);

    Task UpdateSessionRefreshTokenAsync(
        string oldRefreshTokenHash,
        string newRefreshTokenHash,
        DateTime newExpiresAt,
        CancellationToken ct = default);

    Task<bool> ValidateSessionAsync(
        string refreshTokenHash,
        CancellationToken ct = default);

    Task<Guid?> GetSessionIdByRefreshTokenAsync(
        string refreshTokenHash,
        CancellationToken ct = default);

    Task CleanupExpiredSessionsAsync(
        CancellationToken ct = default);
}