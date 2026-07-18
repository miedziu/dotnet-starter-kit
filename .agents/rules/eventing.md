# Eventing — domain events, integration events, Outbox/Inbox

`s/Lib/Eventing/`. Use outbox for publishing.

## Two tiers

- **Domain events** (in-process, pre-commit): inherit `DomainEvent` (`EventId`, `OccurredOnUtc`, `CorrelationId`). Raised on aggregates (`IHasDomainEvents`).
- **Integration events** (cross-module, async): implement `IIntegrationEvent` (`Id`, `OccurredOnUtc`, `CorrelationId`, `Source`). Handlers: `IIntegrationEventHandler<T>`, `sealed`, in `Events/` or `IntegrationEventHandlers/`.

## The Outbox is the only way to publish

```csharp
await _outboxStore.AddAsync(integrationEvent, ct).ConfigureAwait(false);
```

`EfCoreOutboxStore.AddAsync` serializes + `SaveChanges` immediately. `OutboxDispatcherHostedService` polls every `OutboxDispatchIntervalSeconds` (default 10), `OutboxDispatcher` pulls batch (`OutboxBatchSize`, default 100), publishes via `IEventBus`, dead-letters after `OutboxMaxRetries` (default 5) → `IsDead`.

## Idempotency (in-memory bus)

`InMemoryEventBus` applies **Inbox**: skips if `IInboxStore.HasProcessedAsync(eventId, handlerName)`, marks processed after success. Composite key `{Id, HandlerName}`. Don't hand-roll dedup.

## Wiring (3 calls in `ConfigureServices`)

```csharp
services.AddEventingCore(builder.Configuration);
services.AddEventingForDbContext<MyDbContext>();
services.AddIntegrationEventHandlers(typeof(MyModule).Assembly);
```

Bus: `EventingOptions.Provider` = `"RabbitMQ"` → `RabbitMqEventBus`; else `InMemoryEventBus`.

## Gotchas

- **Renaming integration event breaks deserialization** — outbox stores assembly-qualified type name. Keep event type names stable.
- **Background handlers carry no HTTP context.** See `WebhookFanoutHandler`.
- In-memory bus runs handlers **synchronously** in publisher's scope — keep work minimal; exceptions surface to request.
- Set `UseHostedServiceDispatcher=false` to drive outbox via Hangfire.
