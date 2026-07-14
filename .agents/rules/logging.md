# Logging & observability

`src/BuildingBlocks/Web/Observability/`. Use structured logging only.

## Structured logging

**No string interpolation.** Use message templates or `[LoggerMessage]` source-gen for hot paths.

```csharp
// good
_logger.LogInformation("Cleaned up {Count} expired sessions", count);
// hot path (see OutboxDispatcher, InMemoryEventBus, AppHub)
[LoggerMessage(Level = LogLevel.Warning, Message = "Outbox message {MessageId} dead-lettered")]
private partial void LogDeadLettered(Guid messageId);
// NEVER
_logger.LogInformation($"Cleaned up {count} sessions");
```

Build runs with `TreatWarningsAsErrors` — interpolated log calls won't compile.

## Serilog

`AddHeroLogging()` reads `Serilog` config (Console sink by default), attaches `HttpRequestContextEnricher` (adds `RequestMethod`/`RequestPath`/`UserAgent` + `UserId`/`UserEmail` when authenticated), overrides Microsoft/EF/Hangfire to higher levels, excludes `ExceptionHandlerMiddleware` source (global handler logs exceptions itself).

## Correlation

`X-Correlation-ID` header (falls back to `HttpContext.TraceIdentifier`), surfaced in ProblemDetails and pushed to Serilog `LogContext`. `CurrentUserMiddleware` tags `Activity` with `fsh.user_id` / `fsh.correlation_id`.

## OpenTelemetry

`AddHeroOpenTelemetry()` no-ops unless `OpenTelemetryOptions.Enabled`. Metrics + traces for AspNetCore/HttpClient/Npgsql/EFCore/Redis/Runtime, plus caching + auditing meters and Mediator spans.

**OTLP auto-detect:** enabled when `Exporter.Otlp.Enabled=true` OR `OTEL_EXPORTER_OTLP_ENDPOINT` env var present. Aspire injects the env var automatically.

**Logs via OTLP:** `AddHeroLogging` adds `Serilog.Sinks.OpenTelemetry` sink under same auto-detect rule. `service.name` = `OTEL_SERVICE_NAME ?? ApplicationName`.

## Gotchas

- Don't double-log — `ExceptionHandlerMiddleware` source excluded.
- Serilog owns logging pipeline; doesn't forward to other `ILogger` providers.