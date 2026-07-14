# Module: Webhooks

Scoped outbound webhook subscriptions with HMAC-signed delivery and retries. Module `Order = 400`.

**Entities:** `WebhookSubscription` (`Url`, `EventsCsv`, `SecretHash`, `IsActive`), `WebhookDelivery` (per-attempt log). `WebhookDbContext`. Contracts expose **DTOs only**; `IWebhookDispatcher`/`IWebhookDeliveryService` are internal.

**Areas:** Create/Delete/Get subscriptions, GetDeliveries.

## Gotchas

- **Fan-out is open-generic handler** — `WebhookFanoutHandler<TEvent>` handles **every** `IIntegrationEvent` with no per-event wiring. Matches event-type name against `EventsCsv` (`*` wildcard supported).
- **Restore context in background** — fan-out handler and `WebhookDispatchJob` in fresh scope before reading DbContext (background pumps/Hangfire carry no HTTP context). See `eventing.md`, `jobs.md`.
- **HMAC signing** — `X-Webhook-Signature: sha256=<hex HMACSHA256>` (`WebhookPayloadSigner.Sign`), plus `X-Webhook-Event` and `X-Webhook-Delivery-Id` headers.
- **Delivery** — `WebhookDispatcher.EnqueueAsync` enqueues Hangfire `WebhookDispatchJob` per subscription; `[AutomaticRetry(Attempts=4, DelaysInSeconds={30,120,600,3600})]`. Transient (5xx/408/429) throws to reschedule; permanent 4xx completes silently. Each attempt persists `WebhookDelivery` row. `"Webhooks"` HttpClient uses `AddHeroResilience`.