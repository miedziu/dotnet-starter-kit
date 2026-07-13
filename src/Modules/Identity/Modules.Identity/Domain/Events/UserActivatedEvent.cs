using FSH.Framework.Core.Domain;

namespace FSH.Modules.Identity.Domain.Events;

/// <summary>Raised when a user account is activated.</summary>
public sealed record UserActivatedEvent(
    Guid EventId,
    DateTimeOffset OccurredAt,
    string UserId,
    string? ActivatedBy,
    string? CorrelationId = null
) : DomainEvent(EventId, OccurredAt, CorrelationId)
{
    public static UserActivatedEvent Create(string userId, string? activatedBy = null, string? correlationId = null)
        => new(Guid.NewGuid(), DateTimeOffset.UtcNow, userId, activatedBy, correlationId);
}