using FSH.Framework.Core.Domain;

namespace FSH.Mods.Identity.Domain.Events;

/// <summary>Raised when a new user registers in the system.</summary>
public sealed record UserRegisteredEvent(
    Guid EventId,
    DateTimeOffset OccurredAt,
    string UserId,
    string Email,
    string? FirstName,
    string? LastName,
    string? CorrelationId = null) : DomainEvent(EventId, OccurredAt, CorrelationId)
{
    public static UserRegisteredEvent Create(string userId, string email, string? firstName = null, string? lastName = null, string? correlationId = null)
        => new(Guid.NewGuid(), DateTimeOffset.UtcNow, userId, email, firstName, lastName, correlationId);
}