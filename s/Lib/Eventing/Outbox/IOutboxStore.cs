using FSH.Framework.Eventing.Abstractions;

namespace FSH.Framework.Eventing.Outbox;

/// <summary>
/// Abstraction for persisting and reading outbox messages.
/// </summary>
public interface IOutboxStore
{
    Task AddAsync(IIntegrationEvent @event, CancellationToken ct = default);

    /// <summary>
    /// Adds an integration event to the outbox without saving changes.
    /// Use this when you want to include the outbox message in a larger transaction
    /// with other entities. Call DbContext.SaveChangesAsync() separately to commit all changes.
    /// </summary>
    ValueTask AddToContextAsync(IIntegrationEvent @event, CancellationToken ct = default);

    Task<IReadOnlyList<OutboxMessage>> GetPendingBatchAsync(int batchSize, CancellationToken ct = default);

    Task MarkAsProcessedAsync(OutboxMessage message, CancellationToken ct = default);

    Task MarkAsFailedAsync(OutboxMessage message, string error, bool isDead, CancellationToken ct = default);
}