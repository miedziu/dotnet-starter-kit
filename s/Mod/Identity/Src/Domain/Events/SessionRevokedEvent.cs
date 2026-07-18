using FSH.Framework.Core.Domain;

namespace FSH.Mod.Identity.Domain.Events;

/// <summary>Raised when a user session is revoked.</summary>
public sealed record SessionRevokedEvent(
    Guid EventId,
    DateTimeOffset OccurredAt,
    int UserId,
    Guid SessionId,
    string? RevokedBy,
    string? Reason,
    string? CorrelationId = null
) : DomainEvent(EventId, OccurredAt, CorrelationId)
{
    public static SessionRevokedEvent Create(int userId, Guid sessionId, string? revokedBy = null, string? reason = null, string? correlationId = null)
        => new(Guid.NewGuid(), DateTimeOffset.UtcNow, userId, sessionId, revokedBy, reason, correlationId);
}