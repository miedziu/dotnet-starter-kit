<div align="center">

# ⚡ FullStackHero .NET 10 Starter Kit

**A production-ready, modular .NET 10 monolith + one React 19 app — the fastest way to ship a SaaS.**

Identity, billing, auditing, webhooks, files, chat, real-time, caching, jobs, storage, OpenAPI and OpenTelemetry — already wired, and **100% yours as source** (no black-box packages).

[![fsh CLI](https://img.shields.io/nuget/v/FullStackHero.CLI?label=fsh%20cli&color=512BD4)](https://www.nuget.org/packages/FullStackHero.CLI)
[![template](https://img.shields.io/nuget/v/FullStackHero.NET.StarterKit?label=dotnet%20new%20fsh&color=512BD4)](https://www.nuget.org/packages/FullStackHero.NET.StarterKit)
[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4)](https://dotnet.microsoft.com/download)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Docs](https://img.shields.io/badge/docs-fullstackhero.net-2563eb)](https://fullstackhero.net)
[![Stars](https://img.shields.io/github/stars/fullstackhero/dotnet-starter-kit?style=social)](https://github.com/fullstackhero/dotnet-starter-kit)

### [📖 Documentation](https://fullstackhero.net) · [🚀 Get Started](https://fullstackhero.net/docs/getting-started/introduction/) · [🧩 Modules](https://fullstackhero.net/docs/modules/) · [🏗️ Architecture](https://fullstackhero.net/docs/architecture/) · [📦 Changelog](https://fullstackhero.net/docs/changelog/)

</div>

---

## Why FullStackHero?

Most starter kits give you a login page and a TODO list. This one gives you the **boring, hard parts already done right** — auth, billing, auditing, background jobs, real-time, file storage, observability — across a clean **Vertical Slice** backend and one polished **React 19** front-end, orchestrated locally with one command via **.NET Aspire**, and deployable to Docker or AWS.

You scaffold with the `fsh` CLI and get the **complete, detached source** — every BuildingBlock, Module, and Host project with real project references. No hidden NuGet runtime, nothing to "eject" later. Own it, read it, change it.

```bash
dotnet tool install -g FullStackHero.CLI
fsh new MyApp
cd MyApp
dotnet run --project src/Host/FSH.Starter.AppHost   # 🎉 whole stack up: API + React app + Postgres + Redis + MinIO
```

> Then open the **Aspire dashboard** at `https://localhost:15888`, the **API + Scalar docs** at `https://localhost:7030/scalar`, the **dashboard app** at `http://localhost:5174`. Sign in with a seeded demo account (e.g. `admin@acme.com` / `Password123!`).

---

## ✨ What's inside

### Backend — modular monolith, vertical slices
- **.NET 10 · C# latest · Minimal APIs · [Mediator](https://github.com/martinothamar/Mediator) (source-generated CQRS) · FluentValidation**
- **EF Core 10** on **PostgreSQL** (Npgsql), with domain events, the specification pattern, soft-delete + audit interceptors, and `DbContext`s.
- **JWT auth + ASP.NET Identity** — issuance/refresh, roles & fine-grained permissions, rate-limited auth, password policies, sessions, impersonation.
- **Cross-cutting**: HybridCache on **Redis**, **Hangfire** jobs, presigned S3/**MinIO** storage, mailing, idempotency, rate limiting, API versioning, RFC 9457 `ProblemDetails`.
- **Observability**: Serilog structured logging + **OpenTelemetry** traces/metrics/logs, health probes, security/exception auditing.
- **Docs**: **OpenAPI** + the **Scalar** API reference UI.

### Front-end — one React 19 app
- **`clients/dashboard`** (client app): **React 19 + Vite 7 + TypeScript**, **TanStack Query v5**, **React Router 7**, **Radix + Tailwind v4** (shadcn-style), real-time via **SignalR**/**SSE**.
- Runtime config (`/config.json`, no rebuild per environment), hand-written typed API client.

### Modules (bounded contexts)
**Identity · Billing · Tickets · Chat · Files · Webhooks · Auditing · Notifications** — each a runtime project plus a `.Contracts` project (its only public surface).

### Cloud-native & DevOps
- **.NET Aspire** orchestrates the entire stack locally with one command (Redis + RedisInsight, MinIO, migrator, demo-seeder, API, and React app).
- A one-shot **DbMigrator** (migrations are never run at API startup), and the **`fsh` CLI** + `dotnet new` template for distribution.

---

## 🚀 Getting started

### Option 1 — the `fsh` CLI (recommended)

```bash
dotnet tool install -g FullStackHero.CLI
fsh doctor          # verify your environment (SDK, Docker, Aspire, ports)
fsh new MyApp       # interactive wizard
```

The wizard asks what to include (Aspire AppHost, the React app). Non-interactive:

```bash
fsh new MyApp --non-interactive          # full stack, Postgres
fsh new MyApp --no-frontend              # backend-only
fsh new MyApp --no-aspire --no-frontend  # minimal API + migrator
```

### Option 2 — the `dotnet new` template

```bash
dotnet new install FullStackHero.NET.StarterKit
dotnet new fsh -n MyApp
```

### Option 3 — clone the repo

```bash
git clone https://github.com/fullstackhero/dotnet-starter-kit.git MyApp && cd MyApp
dotnet run --project src/Host/FSH.Starter.AppHost
```

> **Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) · [Docker](https://www.docker.com/) (Postgres/Redis/MinIO via Aspire) · [Node 20+](https://nodejs.org/) (for the React app).

**`fsh` commands:** `new` · `doctor` · `info` · `update` · `--version`. Full reference → [fullstackhero.net/docs/cli](https://fullstackhero.net/docs/cli/).

---

## 🧱 Tech stack

| Backend | | Frontend | |
|---|---|---|---|
| Runtime | .NET 10 / C# latest | Framework | React 19 + Vite 7 + TS 5 |
| API | Minimal APIs + Mediator (CQRS) | Data | TanStack Query v5 |
| Validation | FluentValidation | Routing | React Router 7 |
| ORM / DB | EF Core 10 / PostgreSQL | UI | Radix + Tailwind v4 (shadcn) |
| Auth | JWT + ASP.NET Identity | Realtime | SignalR · SSE |
| Cache / Jobs | Redis · Hangfire | | |
| Storage | S3 / MinIO (presigned) | **Infra** | |
| Docs | OpenAPI + Scalar | Orchestration | .NET Aspire |
| Observability | Serilog + OpenTelemetry |

---

## 🗺️ Repository layout

| Path | What |
|---|---|
| `src/BuildingBlocks/` | Shared framework libraries (Core, Persistence, Web, Caching, Eventing, Eventing.Abstractions, Jobs, Mailing, Storage) |
| `src/Modules/{Name}/` | Bounded contexts — each with a runtime project + a `.Contracts` project (its public API) |
| `src/Host/FSH.Starter.Api` | Composition-root Web API host |
| `src/Host/FSH.Starter.AppHost` | .NET Aspire orchestrator (Postgres, Redis, MinIO, migrator, API, React app) |
| `src/Host/FSH.Starter.DbMigrator` | One-shot migrate/seed runner (DB is **not** migrated at API startup) |
| `clients/dashboard` | The React app |

Architecture deep-dive → [fullstackhero.net/docs/architecture](https://fullstackhero.net/docs/architecture/).

---

## 📖 Documentation

Full guides, module references, and architecture decisions live at **[fullstackhero.net](https://fullstackhero.net)**:

- [Getting started](https://fullstackhero.net/docs/getting-started/introduction/) — scaffold, run, and the default credentials
- [Architecture](https://fullstackhero.net/docs/architecture/) — modular monolith + vertical slices
- [Modules](https://fullstackhero.net/docs/modules/) — Identity, Tickets, Chat, and more
- [Local orchestration with Aspire](https://fullstackhero.net/docs/deployment/aspire/)
- [CLI reference](https://fullstackhero.net/docs/cli/) · [Changelog](https://fullstackhero.net/docs/changelog/)

---

## 🤝 Contributing

Issues and PRs are welcome — see [`CONTRIBUTING.md`](CONTRIBUTING.md). Branch from and target **`main`**; CI runs path-scoped backend + frontend pipelines, and stable releases are cut from `v*` tags.

## 📄 License

MIT — see [`LICENSE`](LICENSE). Built and maintained by [**Mukesh Murugan**](https://codewithmukesh.com) and the FullStackHero community, for teams that want to ship fast without sacrificing architectural discipline.

<div align="center">

**[⭐ Star us on GitHub](https://github.com/fullstackhero/dotnet-starter-kit)** if this saves you time — it genuinely helps.

</div>
