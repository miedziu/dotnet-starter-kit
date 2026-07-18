---
name: add-module
description: Create a new module (bounded context) — runtime + Contracts projects, IModule, DbContext, permissions, migrations, and the four registration sites. Use when adding a distinct business domain. For a feature in an existing module, use add-feature.
argument-hint: [ModuleName]
---

# Add Module

**Registration is the footgun** — a module must be wired in **four places**. See `architecture.md`.

## Projects

```
s/Mod/{Name}/
├── Mod.{Name}/            ← runtime: Domain/, Data/, Features/v1/, {Name}Module.cs
└── Mod.{Name}.Contracts/  ← public: v1/, v1/Dtos/, Events/
```

**Copy existing `.csproj` files** — don't hand-write. Runtime refs Contracts + Lib; Contracts refs Mediator + shared contracts.

## Step 1 — `[FshModule]` assembly attribute

```csharp
[assembly: FshModule(typeof(FSH.Mod.{Name}.{Name}Module), 900)]

namespace FSH.Mod.{Name};

public sealed class {Name}Module : IModule
{
    public void ConfigureServices(IHostApplicationBuilder builder)
    {
        PermissionConstants.Register({Name}Permissions.All);
        builder.Services.AddHeroDbContext<{Name}DbContext>();
        builder.Services.AddScoped<IDbInitializer, {Name}DbInitializer>();
        // Eventing (if needed):
        // builder.Services.AddEventingCore(builder.Configuration);
        // builder.Services.AddEventingForDbContext<{Name}DbContext>();
        // builder.Services.AddIntegrationEventHandlers(typeof({Name}Module).Assembly);
        builder.Services.AddHealthChecks().AddDbContextCheck<{Name}DbContext>(name: "db:{name}");
    }
    public void ConfigureMiddleware(IApplicationBuilder app) { }
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var versionSet = endpoints.NewApiVersionSet().HasApiVersion(new(1)).ReportApiVersions().Build();
        var group = endpoints.MapGroup("api/v{version:apiVersion}/{name}")
            .WithTags("{Name}").WithApiVersionSet(versionSet).RequireAuthorization();
        // group.MapCreate{Entity}Endpoint();
    }
}
```

**Order:** Audit 300, File 350, Webhooks 400, Billing 500, Ticket 700, Notifications 750, Chat 800

## Step 2 — Permissions

`{Name}Permissions` with nested resource classes + `All` collection via `PermissionConstants.Register({Name}Permissions.All)`. Mirror `CatalogPermissions`.

## Step 3 — DbContext

```csharp
public sealed class {Name}DbContext : BaseDbContext
{
    public const string Schema = "{name}";
    public {Name}DbContext(DbContextOptions<{Name}DbContext> options) : base(options) { }
    public DbSet<{Entity}> {Entities} => Set<{Entity}>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof({Name}DbContext).Assembly);
        base.OnModelCreating(modelBuilder); // MUST be last
    }
}
```

## Step 4 — Solution + project refs

```bash
dotnet sln add s/Mod/{Name}/Mod.{Name}/Mod.{Name}.csproj
dotnet sln add s/Mod/{Name}/Mod.{Name}.Contracts/Mod.{Name}.Contracts.csproj
```

Add `<ProjectReference>` to runtime from **both** `FSH.Starter.Api` and `DbMigrator`, and from `FSH.Starter.Migrations.PostgreSQL`.

## Step 5 — Migrations folder

Add `{Name}/` folder in `s/Host/FSH.Starter.Migrations.PostgreSQL`, then:

```bash
dotnet ef migrations add Initial --context {Name}DbContext
```

## Step 6 — ⚠️ Register in ALL FOUR places

| Place | File | Markers |
|---|---|---|
| Mediator `o.Assemblies` | Program.cs | Contracts type + module type |
| `moduleAssemblies` | Program.cs | `typeof({Name}Module).Assembly` |
| Mediator assemblies | DbMigrator/Program.cs | Same pair |
| module assemblies | DbMigrator/Program.cs | Same entry |

Miss any → handlers undiscovered, module not loaded, or migrate/seed skips it.

## Step 7 — Verify

```bash
dotnet build s/FSH.Starter.slnx   # 0 warnings
```

## Checklist

- [ ] Two projects (copied csproj), added to `.slnx`, refs from Api + DbMigrator (+ Migrations)
- [ ] `[assembly: FshModule(typeof({Name}Module), order)]`
- [ ] `IModule`: `AddHeroDbContext<T>()`, `PermissionConstants.Register`, version-set group
- [ ] `{Name}DbContext : BaseDbContext`, `base.OnModelCreating` last
- [ ] `{Name}Permissions` in Spec
- [ ] Migrations folder + initial migration
- [ ] **Registered in all four places**
- [ ] Build green
