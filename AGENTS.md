# FullStackHero .NET Starter Kit

**Production-ready modular .NET 10 monolith + React 19 app for enterprise SaaS.**

Canonical guide for AI coding tools. Detailed rules in `.agents/rules/` — read on demand.

## What this is

**Backend:** .NET 10, EF Core 10, PostgreSQL, Redis, JWT + ASP.NET Identity, Hangfire, OpenAPI/Scalar, Serilog + OpenTelemetry, .NET Aspire.

- **Backend** — .NET 10, EF Core 10, PostgreSQL, Redis, JWT + ASP.NET Identity
  Hangfire, OpenAPI/Scalar, Serilog + OpenTelemetry, .NET Aspire.
- **Frontend** — `clients/dashboard`: React 19, Vite 7, TypeScript, TanStack Query v5, React Router 7, Radix + Tailwind v4 (shadcn-style), SignalR/SSE.

**Modules:** Identity, Billing, Tickets, Chat, Files, Webhooks, Auditing, Notifications — each runtime + `.Contracts` project.

## Repo map

| Path | What |
|---|---|
| `src/BuildingBlocks/` | Shared: Core, Persistence, Web, Caching, Eventing, Jobs, Mailing, Storage |
| `src/Modules/{Name}/` | Bounded contexts: runtime + `.Contracts` (public API) |
| `src/Host/FSH.Starter.Api` | Composition-root Web API host |
| `src/Host/FSH.Starter.AppHost` | .NET Aspire orchestrator |
| `src/Host/FSH.Starter.DbMigrator` | One-shot migrate/seed runner |
| `src/Host/FSH.Starter.Migrations.PostgreSQL` | EF migrations (per-module folders) |

## Tech stack

| Component | Tech |
|---|---|
| Framework | .NET 10 / C# latest |
| CQRS | Mediator 3.x (source-gen) |
| Validation | FluentValidation 12.x |
| ORM / DB | EF Core 10 / PostgreSQL |
| Auth | JWT + ASP.NET Identity |
| Cache / Jobs | Redis, Hangfire |
| Docs | OpenAPI + Scalar |
| Hosting | .NET Aspire |

## Build & run

```bash
dotnet run --project src/Host/FSH.Starter.AppHost   # whole stack
dotnet build src/FSH.Starter.slnx                   # build backend
dotnet run --project src/Host/FSH.Starter.Api       # API only → https://localhost:7030
```

Migrations: `dotnet run --project DbMigrator -- apply [--seed]`

**Ports:** API 7030 (https)/5030 (http) · Postgres 5432 · Redis 6379 · MinIO 9000/9001.

## Golden rules

1. **Module boundaries** — reference only `.Contracts`, never runtime project
2. **Registration touches FOUR places** — Program.cs + DbMigrator/Program.cs (Mediator assemblies + moduleAssemblies). Miss one → silent failure.
3. **BuildingBlocks** — shared by all modules, wide blast radius
4. **Handlers:** `public sealed`, `ValueTask<T>`, `.ConfigureAwait(false)`
5. **Structured logging only** — no string interpolation; use message templates
6. **Propagate `CancellationToken`** — add as `= default` on public methods
7. **Every command handler + paginated query needs a validator**
8. **Docs + changelog travel with the change**

## Rules index

| Working on… | Read |
|---|---|
| Module structure, boundaries, registration, DI, middleware order, config | `architecture.md` |
| Endpoints, CQRS, validation, exceptions, permissions, versioning | `api-conventions.md` |
| EF Core, entities, migrations, query filters | `database.md` |
| Cross-module events, Outbox/Inbox, idempotent handlers | `eventing.md` |
| Caching (HybridCache/Redis), keys, invalidation | `caching.md` |
| Background jobs (Hangfire), recurring jobs | `jobs.md` |
| Outbound HTTP resilience (Polly) | `resilience.md` |
| Files/blobs, presigned uploads, providers | `storage.md` |
| CORS, security headers, rate limiting, idempotency | `security.md` |
| SignalR / SSE backend | `realtime.md` |
| Logging, correlation, OpenTelemetry | `logging.md` |
| **Modifying `src/BuildingBlocks`** (read first) | `buildingblocks-protection.md` |
| A specific module's quirks | `modules/{module}.md` (identity, chat, files, webhooks, auditing, billing, catalog, tickets, notifications) |

**Frontend** (`.agents/rules/frontend/`)

| Working on… | Read |
|---|---|
| Any React work (shared stack, API client, Query, Tailwind, design language) | `frontend/shared.md` |
| The client app (`clients/dashboard`) | `frontend/dashboard.md` |

## Skills

- `add-feature` — feature in existing module
- `add-module` — new bounded context
- `add-entity` — EF entity
- `add-react-page` — frontend page
- `query-patterns` — GET endpoints
- `mediator-reference` — CQRS interfaces

## Coding style (backend)

File-scoped namespaces · 4-space indent · explicit types · `is null` / `is not null` · pattern matching · `ArgumentNullException.ThrowIfNull` guards · records for DTOs/events · `TreatWarningsAsErrors`
