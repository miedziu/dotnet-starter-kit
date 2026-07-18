using FSH.Framework.Eventing.Abstractions;

namespace FSH.Mod.Identity.Spec.Events;

/// <summary>
/// Integration event raised when a JWT token is generated for a user.
/// Intended primarily as a sample event to exercise the eventing/outbox pipeline.
/// </summary>
public sealed record TokenGeneratedIntegrationEvent(
    Guid Id,
    DateTime OccurredAt,
    string CorrelationId,
    string Source,
    string UserId,
    string Email,
    string ClientId,
    string IpAddress,
    string UserAgent,
    string TokenFingerprint,
    DateTime AccessTokenExpiresAtUtc)
    : IIntegrationEvent;