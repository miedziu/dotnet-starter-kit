# FullStackHero .NET Starter Kit

> A production-ready modular .NET 10 monolith + React 19 app, built for enterprise SaaS.

This file is the canonical guide for **all** AI coding tools (Claude Code, Gemini CLI, Cursor, Codex, …).
`CLAUDE.md` and `GEMINI.md` are thin bridges that import this file — edit conventions **here**, not there.

This file is the map. Detailed conventions live in `.agents/rules/` and are read on demand — **read the
relevant rule file before working in that area** (see the index below). Keep this file lean.

## What this is

A **modular monolith** (Vertical Slice Architecture) backend that ships with **React + Vite**
front-end. Auth, auditing, billing, files, chat and more are first-class.

- **Backend** — .NET 10, EF Core 10, PostgreSQL, Redis, JWT + ASP.NET Identity
  Hangfire, OpenAPI/Scalar, Serilog + OpenTelemetry, .NET Aspire.
- **Frontend** — `clients/dashboard`: React 19, Vite 7, TypeScript, TanStack Query v5, React Router 7, Radix + Tailwind v4 (shadcn-style), SignalR/SSE.

## Repo map

| Path | What |
|------|------|
| `src/BuildingBlocks/` | Shared framework libraries (Core, Persistence, Web, Caching, Eventing, Jobs, Mailing, Storage, Shared…). |
| `src/Modules/{Name}/` | Bounded contexts. Each has a runtime project + a `.Contracts` project (its only public API). |
| `src/Host/FSH.Starter.Api` | Composition-root Web API host. |
| `src/Host/FSH.Starter.AppHost` | .NET Aspire orchestrator (Redis, MinIO, migrator, API). |
| `src/Host/FSH.Starter.DbMigrator` | One-shot migrate/seed runner. DB is **not** migrated at API startup. |
| `src/Host/FSH.Starter.Migrations.PostgreSQL` | All EF migrations, organized per-module by folder. |
| `clients/dashboard` | The React app. |

## Tech stack

| Backend | | Frontend | |
|---|---|---|---|
| Framework | .NET 10 / C# latest | Framework | React 19 + Vite 7 + TS 5.x |
| CQRS | Mediator 3.x (source-gen) | Data | TanStack Query v5 |
| Validation | FluentValidation 12.x | Routing | React Router 7 |
| ORM / DB | EF Core 10 / PostgreSQL (Npgsql) | UI | Radix + Tailwind v4 + CVA (shadcn) |
| Auth | JWT Bearer + ASP.NET Identity | Forms | react-hook-form |
| Realtime | `@microsoft/signalr`, SSE (dashboard) |
| Cache / Jobs | Redis, Hangfire |
| Docs | OpenAPI + Scalar | API client | hand-written `apiFetch` (no codegen) |
| Hosting | .NET Aspire | Env | runtime `/config.json` (not `VITE_*`) |

## Build & run

```bash
# Stack (Redis + MinIO + migrator + API + dashboard React app)
dotnet run --project src/Host/FSH.Starter.AppHost   # one-time: npm install in clients/dashboard

dotnet build src/FSH.Starter.slnx                   # build backend
dotnet run --project src/Host/FSH.Starter.Api       # API only → https://localhost:7030 (/scalar)

cd clients/dashboard && npm install && npm run dev       # → http://localhost:5174
```

Migrations / seed (DbMigrator, separate step):
```bash
dotnet run --project src/Host/FSH.Starter.DbMigrator -- apply [--seed]
dotnet run --project src/Host/FSH.Starter.DbMigrator -- list-pending
```

**Ports:** API 7030 (https)/5030 (http) · dashboard 5174 · Postgres 5432 · Redis 6379 · MinIO 9000/9001.

## Branching

Single long-lived branch: **`main`** (the default) — there is **no `develop`**. Branch from and target `main`; stable releases are cut from `v*` tags.

## Golden rules (do not break)

1. **Module boundaries** — a module references another module only through its `.Contracts` project, never its runtime project.
2. **Registering a module touches FOUR places** — `Program.cs` Mediator `o.Assemblies` (two markers each) + `moduleAssemblies` array, **and the identical pair in `DbMigrator/Program.cs`**. A missing Mediator marker = handlers silently undiscovered. See `architecture.md`.
3. **`src/BuildingBlocks`**— shared by every module, wide blast radius.
4. **Mediator handlers must be `public sealed`**, return `ValueTask<T>`, and `.ConfigureAwait(false)` every await.
5. **Structured logging only** — no string interpolation in log messages; use message templates / `[LoggerMessage]`.
6. **Propagate `CancellationToken`** into every EF/IO call; add as `= default` on public service methods.
7. **Every command handler + paginated query handler needs a validator** (`{Name}Validator`).
8. **Frontend: pass per-call data through `mutate(arg)`**, never via state the mutation callbacks close over (execute-time race). See `frontend/shared.md`.
9. **Docs + changelog travel with the change** — a user-facing change (feature, endpoint, config, infra, breaking change) isn't done until the **separate docs repo** (`github.com/fullstackhero/docs`, the Astro site) is updated to match **and** a changelog entry is added (`src/content/docs/changelog/`). Don't let the docs drift from the code.

## Rules index — read the relevant file before you work

**Backend / cross-cutting** (`.agents/rules/`)

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

## Coding style (backend)

File-scoped namespaces · 4-space indent · explicit types (`var` only when RHS-obvious) · `is null` /
`is not null` · pattern matching + switch expressions · `ArgumentNullException.ThrowIfNull` guards ·
records for DTOs/events/value objects · `default!` for required non-nullable strings. Build runs with
`TreatWarningsAsErrors` — warnings fail the build.

## Adding things (quick pointers)

- **Feature** — Contracts command/query → handler → validator → endpoint → wire in module `MapEndpoints()`. Details: `api-conventions.md`.
- **Module** — new `Modules.{Name}` + `.Contracts`, implement `IModule` w/ assembly-level `[assembly: FshModule(typeof(XModule), order)]`, register in **all four places**, add migration folder. Details: `architecture.md`.
- **React page** — API module (`src/api/`) → page → register lazy route → (admin) mirror permission + RouteGuard. Details: `frontend/shared.md`.

## AI tooling resources

- **Rules** — `.agents/rules/*.md` (indexed above). Read on demand.
- **Skills** — `.agents/skills/*/SKILL.md`: step-by-step task recipes. Scaffolders: `add-feature`, `add-entity`, `add-module`, `add-react-page`, `add-full-slice`. Ops: `create-migration`, `add-integration-event`, `add-permission`. Reference: `query-patterns`, `mediator-reference`.
- **Workflows** — `.agents/workflows/*.md`: task playbooks (`code-reviewer`, `feature-scaffolder`, `module-creator`, `architecture-guard`, `migration-helper`).
