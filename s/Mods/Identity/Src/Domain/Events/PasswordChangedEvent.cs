using FSH.Framework.Core.Domain;

namespace FSH.Mods.Identity.Domain.Events;

/// <summary>Raised when a user changes their password.</summary>
public sealed record PasswordChangedEvent(
    Guid EventId,
    DateTimeOffset OccurredAt,
    string UserId,
    bool WasReset,
    string? CorrelationId = null
) : DomainEvent(EventId, OccurredAt, CorrelationId)
{
    public static PasswordChangedEvent Create(string userId, bool wasReset = false, string? correlationId = null)
        => new(Guid.NewGuid(), DateTimeOffset.UtcNow, userId, wasReset, correlationId);
}