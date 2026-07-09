# FSH.Starter

Your application, generated from the **FSH .NET Starter Kit** — a production-ready modular
.NET 10 monolith with one React 19 app, identity, background jobs, and
cloud-native deploy.

You **own all of this source**. There are no framework NuGet packages to track or upgrade —
the shared code lives in `src/BuildingBlocks` and is yours to change.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 20+](https://nodejs.org/) — for the React app
- [Docker](https://www.docker.com/) — Postgres, Redis, MinIO (orchestrated by Aspire)

## Quick start

### Everything at once (recommended) — .NET Aspire

```bash
dotnet run --project src/Host/FSH.Starter.AppHost
```

Aspire starts Postgres, Redis, and MinIO, runs database migrations, then launches the API
and the React app.

| Surface | URL |
|---|---|
| Aspire dashboard | https://localhost:15888 |
| API + Scalar docs | https://localhost:7030/scalar |
| Dashboard app | http://localhost:5174 |

### Backend only

```bash
dotnet run --project src/Host/FSH.Starter.Api      # needs external Postgres + Redis
```

### Frontend only (against a running API)

```bash
cd clients/dashboard && npm install && npm run dev   # → http://localhost:5174
```

The React app reads its API URL at runtime from `public/config.json` — no rebuild to repoint.

## Project structure

```
src/
  BuildingBlocks/      Shared framework libraries — yours to modify
  Modules/             Bounded contexts: Identity, Auditing, Billing,
                       Catalog, Chat, Files, Notifications, Tickets, Webhooks
  Host/
    FSH.Starter.Api/                    API composition root
    FSH.Starter.AppHost/                .NET Aspire orchestrator
    FSH.Starter.DbMigrator/             One-shot migrate / seed runner
    FSH.Starter.Migrations.PostgreSQL/  EF Core migrations
clients/
  dashboard/           Client app (React 19 + Vite + Tailwind)
```

## Database

Migrations run automatically under Aspire. To apply them yourself:

```bash
dotnet run --project src/Host/FSH.Starter.DbMigrator -- apply --seed
```

## Make it yours — first-run checklist

This project shipped with sensible defaults. Before production:

- [ ] **Logo** — replace `clients/dashboard/public/logo-fullstackhero.png` with your own.
- [ ] **Mail** — configure SMTP / SendGrid under `MailOptions` in
      `src/Host/FSH.Starter.Api/appsettings.json`.
- [ ] **OpenAPI contact** — update `OpenApiOptions.Contact` in `appsettings.json`.

Sign in to the admin console as `admin@root.com` using the `SEED_ADMIN_PASSWORD` from your
`.env`, then rotate it from Settings → Security.

## Adding a feature

1. Contracts command/query in `src/Modules/{Module}.Contracts/v1/{Area}/{Feature}/`
2. Handler + FluentValidation validator in `src/Modules/{Module}/Features/...`
3. Endpoint, wired into the module's `MapEndpoints()`

## Learn more

- [FSH Documentation](https://fullstackhero.net)
- [Source & issues](https://github.com/fullstackhero/dotnet-starter-kit)