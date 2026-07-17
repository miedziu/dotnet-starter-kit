using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Identity.Contracts.v1.Dtos;
using FSH.Modules.Identity.Data;
using FSH.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;
using UAParser;

namespace FSH.Modules.Identity.Services;

public sealed class SessionService : ISessionService
{
    private readonly IdentityDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<SessionService> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly Parser _uaParser;

    public SessionService(
        IdentityDbContext db,
        ICurrentUser currentUser,
        ILogger<SessionService> logger,
        TimeProvider timeProvider)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
        _timeProvider = timeProvider;
        _uaParser = Parser.GetDefault();
    }

    public async Task<UserSessionDto> CreateSessionAsync(
        string userId,
        string refreshTokenHash,
        string ipAddress,
        string userAgent,
        DateTime expiresAt,
        CancellationToken ct = default)
    {
        // Fetch IntId from the user
        var intId = await _db.Users
            .Where(u => u.Id == userId)
            .Select(u => u.IntId)
            .FirstOrDefaultAsync(ct);

        if (intId == 0)
        {
            throw new NotFoundException($"User with ID '{userId}' not found.");
        }

        var clientInfo = _uaParser.Parse(userAgent);

        var session = UserSession.Create(
            userId: intId,
            refreshTokenHash: refreshTokenHash,
            ipAddress: ipAddress,
            userAgent: userAgent,
            expiresAt: expiresAt,
            deviceType: DeviceTypeClassifier.Classify(clientInfo.Device.Family),
            browser: clientInfo.UA.Family,
            browserVersion: clientInfo.UA.Major,
            operatingSystem: clientInfo.OS.Family,
            osVersion: clientInfo.OS.Major);

        _db.UserSessions.Add(session);
        await _db.SaveChangesAsync(ct);

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Created session {SessionId} for user {UserId}", session.Id, userId);
        }

        return MapToDto(session, isCurrentSession: true);
    }

    public async Task<List<UserSessionDto>> GetUserSessionsAsync(
        string userId,
        CancellationToken ct = default)
    {
        var currentUserId = _currentUser.GetUserId().ToString();
        if (!string.Equals(userId, currentUserId, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Cannot view sessions for another user");
        }

        // Fetch IntId from the user
        var intId = await _db.Users
            .Where(u => u.Id == userId)
            .Select(u => u.IntId)
            .FirstOrDefaultAsync(ct);

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var sessions = await _db.UserSessions
            .AsNoTracking()
            .Where(s => s.UserId == intId && !s.IsRevoked && s.ExpiresAt > now)
            .OrderByDescending(s => s.LastActivityAt)
            .ToListAsync(ct);

        return sessions.Select(s => MapToDto(s, isCurrentSession: false)).ToList();
    }

    public async Task<List<UserSessionDto>> GetUserSessionsForAdminAsync(
        string userId,
        CancellationToken ct = default)
    {
        // Fetch IntId from the user
        var intId = await _db.Users
            .Where(u => u.Id == userId)
            .Select(u => u.IntId)
            .FirstOrDefaultAsync(ct);

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var sessions = await _db.UserSessions
            .AsNoTracking()
            .Include(s => s.User)
            .Where(s => s.UserId == intId && !s.IsRevoked && s.ExpiresAt > now)
            .OrderByDescending(s => s.LastActivityAt)
            .ToListAsync(ct);

        return sessions.Select(s => MapToDto(s, isCurrentSession: false)).ToList();
    }

    public async Task<(List<UserSessionDto> Items, long TotalCount)> GetAllSessionsAsync(
        bool includeInactive,
        string? search,
        int skip,
        int take,
        CancellationToken ct = default)
    {
        // Cap server-side so an over-eager client can't pull the full
        // session table in one round-trip.
        if (take is < 1 or > 200) take = 50;
        if (skip < 0) skip = 0;

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var q = _db.UserSessions
            .AsNoTracking()
            .Include(s => s.User)
            .AsQueryable();

        if (!includeInactive)
        {
            q = q.Where(s => !s.IsRevoked && s.ExpiresAt > now);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            string term = search.Trim();
            q = q.Where(s =>
                (s.User != null && s.User.UserName != null && EF.Functions.ILike(s.User.UserName, $"%{term}%"))
                || (s.User != null && s.User.Email != null && EF.Functions.ILike(s.User.Email, $"%{term}%"))
                || (s.IpAddress != null && EF.Functions.ILike(s.IpAddress, $"%{term}%")));
        }

        long total = await q.LongCountAsync(ct);

        var sessions = await q
            .OrderByDescending(s => s.LastActivityAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        return (sessions.Select(s => MapToDto(s, isCurrentSession: false)).ToList(), total);
    }

    public async Task<UserSessionDto?> GetSessionAsync(
        Guid sessionId,
        CancellationToken ct = default)
    {
        var session = await _db.UserSessions
            .AsNoTracking()
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == sessionId, ct);

        return session is null ? null : MapToDto(session, isCurrentSession: false);
    }

    public async Task<bool> RevokeSessionAsync(
        Guid sessionId,
        string revokedBy,
        string? reason = null,
        CancellationToken ct = default)
    {
        var session = await _db.UserSessions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == sessionId && !s.IsRevoked, ct);

        if (session is null)
        {
            return false;
        }

        var currentUserId = _currentUser.GetUserId().ToString();
        if (!string.Equals(session.UserId.ToString(CultureInfo.InvariantCulture), currentUserId, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Cannot revoke session for another user");
        }

        session.Revoke(revokedBy, reason ?? "User requested");

        await _db.SaveChangesAsync(ct);

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Session {SessionId} revoked by {RevokedBy}", sessionId, revokedBy);
        }

        return true;
    }

    public async Task<int> RevokeAllSessionsAsync(
        string userId,
        string revokedBy,
        Guid? exceptSessionId = null,
        string? reason = null,
        CancellationToken ct = default)
    {
        var currentUserId = _currentUser.GetUserId().ToString();
        if (!string.Equals(userId, currentUserId, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Cannot revoke sessions for another user");
        }

        // Fetch IntId from the user
        var intId = await _db.Users
            .Where(u => u.Id == userId)
            .Select(u => u.IntId)
            .FirstOrDefaultAsync(ct);

        var query = _db.UserSessions
            .Where(s => s.UserId == intId && !s.IsRevoked);

        if (exceptSessionId.HasValue)
        {
            query = query.Where(s => s.Id != exceptSessionId.Value);
        }

        var sessions = await query.ToListAsync(ct);

        foreach (var session in sessions)
        {
            session.Revoke(revokedBy, reason ?? "User requested logout from all devices");
        }

        await _db.SaveChangesAsync(ct);

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Revoked {Count} sessions for user {UserId}", sessions.Count, userId);
        }

        return sessions.Count;
    }

    public async Task<int> RevokeAllSessionsForAdminAsync(
        string userId,
        string revokedBy,
        string? reason = null,
        CancellationToken ct = default)
    {
        // Fetch IntId from the user
        var intId = await _db.Users
            .Where(u => u.Id == userId)
            .Select(u => u.IntId)
            .FirstOrDefaultAsync(ct);

        var sessions = await _db.UserSessions
            .Where(s => s.UserId == intId && !s.IsRevoked)
            .ToListAsync(ct);

        foreach (var session in sessions)
        {
            session.Revoke(revokedBy, reason ?? "Admin requested");
        }

        await _db.SaveChangesAsync(ct);

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Admin {AdminId} revoked {Count} sessions for user {UserId}",
                revokedBy, sessions.Count, userId);
        }

        return sessions.Count;
    }

    public async Task<bool> RevokeSessionForAdminAsync(
        Guid sessionId,
        string revokedBy,
        string? reason = null,
        CancellationToken ct = default)
    {
        var session = await _db.UserSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && !s.IsRevoked, ct);

        if (session is null)
        {
            return false;
        }

        session.Revoke(revokedBy, reason ?? "Admin requested");

        await _db.SaveChangesAsync(ct);

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Admin {AdminId} revoked session {SessionId}", revokedBy, sessionId);
        }

        return true;
    }

    public async Task UpdateSessionActivityAsync(
        string refreshTokenHash,
        CancellationToken ct = default)
    {
        var session = await _db.UserSessions
            .FirstOrDefaultAsync(s => s.RefreshTokenHash == refreshTokenHash && !s.IsRevoked, ct);

        if (session is not null)
        {
            session.UpdateActivity();
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task UpdateSessionRefreshTokenAsync(
        string oldRefreshTokenHash,
        string newRefreshTokenHash,
        DateTime newExpiresAt,
        CancellationToken ct = default)
    {
        var session = await _db.UserSessions
            .FirstOrDefaultAsync(s => s.RefreshTokenHash == oldRefreshTokenHash && !s.IsRevoked, ct);

        if (session is not null)
        {
            session.UpdateRefreshToken(newRefreshTokenHash, newExpiresAt);
            await _db.SaveChangesAsync(ct);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Updated session {SessionId} with new refresh token", session.Id);
            }
        }
    }

    public async Task<bool> ValidateSessionAsync(
        string refreshTokenHash,
        CancellationToken ct = default)
    {
        var session = await _db.UserSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.RefreshTokenHash == refreshTokenHash, ct);

        if (session is null)
        {
            return true; // No session tracking for this token (backwards compatibility)
        }

        return !session.IsRevoked && session.ExpiresAt > _timeProvider.GetUtcNow().UtcDateTime;
    }

    public async Task<Guid?> GetSessionIdByRefreshTokenAsync(
        string refreshTokenHash,
        CancellationToken ct = default)
    {
        var session = await _db.UserSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.RefreshTokenHash == refreshTokenHash && !s.IsRevoked, ct);

        return session?.Id;
    }

    public async Task CleanupExpiredSessionsAsync(
        CancellationToken ct = default)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var cutoffDate = now.AddDays(-30); // Keep revoked sessions for 30 days for audit
        var deleted = await _db.UserSessions
            .Where(s => s.ExpiresAt < now && s.ExpiresAt < cutoffDate)
            .ExecuteDeleteAsync(ct);

        if (deleted > 0 && _logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Cleaned up {Count} expired sessions", deleted);
        }
    }

    private UserSessionDto MapToDto(UserSession session, bool isCurrentSession)
    {
        return new UserSessionDto
        {
            Id = session.Id,
            UserId = session.User?.Id,
            UserName = session.User?.UserName,
            UserEmail = session.User?.Email,
            IpAddress = session.IpAddress,
            DeviceType = session.DeviceType,
            Browser = session.Browser,
            BrowserVersion = session.BrowserVersion,
            OperatingSystem = session.OperatingSystem,
            OsVersion = session.OsVersion,
            CreatedAt = session.CreatedAt,
            LastActivityAt = session.LastActivityAt,
            ExpiresAt = session.ExpiresAt,
            IsActive = !session.IsRevoked && session.ExpiresAt > _timeProvider.GetUtcNow().UtcDateTime,
            IsCurrentSession = isCurrentSession
        };
    }
}