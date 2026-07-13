using Mediator;

namespace FSH.Framework.Core.Domain;

/// <summary>
/// Represents a domain event with correlation context.
/// Extends <see cref="INotification"/> so domain events can be published via Mediator.
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// Gets the unique event identifier.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Gets the UTC timestamp when the event occurred.
    /// </summary>
    DateTimeOffset OccurredAt { get; }

    /// <summary>
    /// Gets the correlation identifier for tracing across boundaries.
    /// </summary>
    string? CorrelationId { get; }
}