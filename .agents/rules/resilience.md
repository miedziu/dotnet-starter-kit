# HTTP resilience

`s/Lib/Web/HttpResilience/`. Uses `Microsoft.Extensions.Http.Resilience` (Polly v8).

## Pattern — opt-in per HttpClient

`AddHeroResilience(config)` adds `AddStandardResilienceHandler` from `HttpResilienceOptions` (retry, timeout, circuit breaker). **NOT global** — chain onto specific outbound client:

```csharp
builder.Services.AddHttpClient("Webhooks", ...)
    .AddHeroResilience(builder.Configuration);
```

Defaults: 3 retries, 30s total, 10s per attempt, 50% failure ratio, throughput 10. No-ops when `Enabled=false`.

## Notes

- Only for outbound integrations. Current caller: Webhooks delivery client. Add to any new `HttpClient` calling flaky external service.
- For background work, prefer Hangfire's `[AutomaticRetry]` (durable across restarts); resilience handler only covers in-flight HTTP call.
