# Background jobs (Hangfire)

`s/Lib/Jobs/`. Use `IJobService` for fire-and-forget/scheduled; `IRecurringJobManager` for recurring.

## Fire-and-forget / scheduled

Inject `IJobService` (`Jobs/Services/IJobService.cs`) — don't call Hangfire's `BackgroundJob` directly.

```csharp
jobService.Enqueue(() => mailService.SendAsync(req, CancellationToken.None));   // default queue
jobService.Enqueue("email", () => mailService.SendAsync(req, CancellationToken.None));
jobService.Schedule(() => DoLater(), TimeSpan.FromMinutes(5));
```

Queues: `default`, `email` (5 workers, 30s poll). Storage from `DatabaseOptions.Provider`.

## Recurring jobs

`IJobService` has no recurring API. Register in module's `MapEndpoints` with `IRecurringJobManager.AddOrUpdate<T>(...)`, always `TimeZoneInfo.Utc`:

```csharp
recurringJobs.AddOrUpdate<PurgeOrphanedFilesJob>("files:purge-orphaned",
    j => j.RunAsync(CancellationToken.None), Cron.Hourly(), new() { TimeZone = TimeZoneInfo.Utc });
```

Examples: `PurgeOrphanedFiles`/`PurgeDeletedFiles` (Files), `MonthlyInvoiceJob` (Billing), `AuditRetentionJob` (Audit), `WebhookDispatchJob` (Webhooks).

## Dashboard & config

`/jobs` (default), behind `HangfireOptions.UserName`/`Password` basic auth — both `[Required]`, password `[MinLength(12)]`. Startup fails in non-dev if unset.

## Gotchas

- `DbMigrator` registers `NoOpJobService` whose methods **throw** — surfaces accidental enqueue during migration. Don't enqueue from migration/seed.
- Job class is DI-resolved type (scope-per-job via `FshJobActivator`); inject what you need.
