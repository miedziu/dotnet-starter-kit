using FSH.Framework.Core.Domain;

namespace FSH.Modules.Identity.Domain.Events;

/// <summary>Raised when a user account is deactivated.</summary>
public sealed record UserDeactivatedEvent(
    Guid EventId,
    DateTimeOffset OccurredAt,
    string UserId,
    string? DeactivatedBy,
    string? Reason,
    string? CorrelationId = null
) : DomainEvent(EventId, OccurredAt, CorrelationId)
{
    public static UserDeactivatedEvent Create(string userId, string? deactivatedBy = null, string? reason = null, string? correlationId = null)
        => new(Guid.NewGuid(), DateTimeOffset.UtcNow, userId, deactivatedBy, reason, correlationId);
}