# Database & EF Core

## Entities

- `BaseEntity`: `Id`, `CreatedAt`, `UpdatedAt`
- `AggregateRoot`: `BaseEntity` + domain events (`IHasDomainEvents`, `_domainEvents`)
- Markers: `IAuditableEntity`, `ISoftDeletable`
- Domain events: `DomainEvent` (record: `EventId`, `OccurredOnUtc`, `CorrelationId`)
- Integration events: `IIntegrationEvent` + `IIntegrationEventHandler<T>`

## AsNoTracking

- Read-only: `.AsNoTracking()` (Specifications default to it)
- **Don't** add to read-then-mutate-then-save — entity must stay tracked
- `AnyAsync(...)` doesn't materialize entity → skip `AsNoTracking()` there

## Value generation for nav children

Child entity reached **only** via parent nav collection needs `Property(x => x.Id).ValueGeneratedNever()` — otherwise EF treats as `Modified` instead of `Added`.

## Migrations

All in `s/Host/FSH.Starter.Migrations.PostgreSQL`, per-module folders.

**Command:**
```bash
dotnet ef migrations add {Name} --context {Module}DbContext
```

- DB not migrated at startup; use `DbMigrator`: `apply` (default), `seed`, `seed-demo`, `list-pending`
- Run `dotnet tool restore` first

## Migrations workflow

Always recreate existing migrations with same filename. Build before `migrations add` to keep snapshot current.
