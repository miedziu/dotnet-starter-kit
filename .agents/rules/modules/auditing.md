# Module: Auditing

Append-only audit trail (entity changes, security events, exceptions, HTTP activity) with async channel-buffered persistence + DLQ. Module `Order = 300`.

**Entities:** `AuditRecord`, `AuditDbContext`. `AuditEnvelope` is in-flight event. Rich Contracts: `IAuditClient`, `ISecurityAudit`, `IAuditPublisher`, `IAuditSink`, `IAuditDlqSink`, `IAuditEnricher`, `NoAuditAttribute`, payload records.

**Areas:** read-only query side — GetAudits / ByCorrelation / ByTrace / Summary / Exception / Security.

## Gotchas

- **Static `Audit` fluent API** — `Audit.ForSecurity(...).WithUser(...).WriteAsync(ct)` (also `ForEntityChange`/`ForActivity`/`ForException`). Configured once via `Audit.Configure(publisher, serializer, enrichers)`. Enrichers held in **volatile immutable array swapped atomically** — never mutate live enricher list.
- **Two interceptors:** `AuditingSaveChangesInterceptor` (this module) captures EF diffs → EntityChange events, skips `AuditDbContext`. `AuditableEntitySaveChangesInterceptor` (BuildingBlocks) stamps audit/soft-delete fields.
- **Channel-buffered, never blocks request** — `ChannelAuditPublisher` has two lanes: default (`DropOldest` under pressure) and **security lane that back-pressures and never drops**. `AuditBackgroundWorker` drains both (security first), batches, writes via `IAuditSink`; on failure retries then spills to `IAuditDlqSink` (file).
- `SqlAuditSink` sets context per group in fresh scope (null → Root).
- **JSON masking** redacts fields by keyword (password/secret/token/apiKey/connectionString…) → `****`.
- Exclude endpoint from activity auditing with `[NoAudit]` / `NoAudit` endpoint extension.