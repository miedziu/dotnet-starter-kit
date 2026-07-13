using FSH.Framework.Eventing.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FSH.Framework.Eventing.Outbox;

/// <summary>
/// EF Core-based outbox store for a specific DbContext.
/// </summary>
/// <typeparam name="TDbContext">The DbContext that owns the OutboxMessages set.</typeparam>
public sealed class EfCoreOutboxStore<TDbContext> : IOutboxStore
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;
    private readonly IEventSerializer _serializer;
    private readonly ILogger<EfCoreOutboxStore<TDbContext>> _logger;
    private readonly TimeProvider _timeProvider;

    public EfCoreOutboxStore(
        TDbContext dbContext,
        IEventSerializer serializer,
        ILogger<EfCoreOutboxStore<TDbContext>> logger,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _serializer = serializer;
        _logger = logger;
        _timeProvider = timeProvider;
    }

    public async Task AddAsync(IIntegrationEvent @event, CancellationToken ct = default)
    {
        await AddToContextAsync(@event, ct).ConfigureAwait(false);
        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public ValueTask AddToContextAsync(IIntegrationEvent @event, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var payload = _serializer.Serialize(@event);
        var message = new OutboxMessage
        {
            Id = @event.Id,
            CreatedAt = @event.OccurredAt,
            Type = @event.GetType().AssemblyQualifiedName ?? @event.GetType().FullName!,
            Payload = payload,
            CorrelationId = @event.CorrelationId,
            RetryCount = 0,
            IsDead = false
        };

        _dbContext.Set<OutboxMessage>().Add(message);
        return default;
    }

    public async Task<IReadOnlyList<OutboxMessage>> GetPendingBatchAsync(int batchSize, CancellationToken ct = default)
    {
        return await _dbContext.Set<OutboxMessage>()
            .Where(m => !m.IsDead && m.ProcessedAt == null)
            .OrderBy(m => m.CreatedAt)
            .Take(batchSize)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task MarkAsProcessedAsync(OutboxMessage message, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        message.ProcessedAt = _timeProvider.GetUtcNow().UtcDateTime;
        _dbContext.Set<OutboxMessage>().Update(message);
        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task MarkAsFailedAsync(OutboxMessage message, string error, bool isDead, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        message.RetryCount++;
        message.LastError = error;
        message.IsDead = isDead;

        _logger.LogWarning("Outbox message {MessageId} failed. RetryCount={RetryCount}, IsDead={IsDead}, Error={Error}",
            message.Id, message.RetryCount, message.IsDead, error);

        _dbContext.Set<OutboxMessage>().Update(message);
        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }
}