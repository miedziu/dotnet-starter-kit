# Architecture

**Layers:** Host → Modules.{Name} → Modules.{Name}.Contracts → BuildingBlocks

**Module = runtime + Contracts**
- Runtime: handlers, services, domain, data
- Contracts: commands, queries, events, DTOs, interfaces

Cross-module comms: Contracts service interfaces or integration events only.

## Feature layout (Single File Slice)
Features/v1/{Area}/{Feature}.cs

## IModule registration

**Assembly-level:** `[assembly: FshModule(typeof(XModule), order)]` (not class-level)

```csharp
[assembly: FshModule(typeof(FSH.Modules.Identity.IdentityModule), 1)]

namespace FSH.Modules.Identity;

public sealed class IdentityModule : IModule
{
    public void ConfigureServices(IHostApplicationBuilder builder) { }
    public void ConfigureMiddleware(IApplicationBuilder app) { }
    public void MapEndpoints(IEndpointRouteBuilder endpoints) { }
}
```

## ⚠️ Four-place registration (the footgun)

| Place | File | Symptom |
|---|---|---|
| Mediator `o.Assemblies` (2 markers) | Program.cs | Handlers undiscovered |
| `moduleAssemblies` array | Program.cs | Module not loaded |
| Mediator assemblies | DbMigrator/Program.cs | Migrate/seed misses module |
| module assemblies array | DbMigrator/Program.cs | Migrate/seed misses module |

## DI & handlers

- `public sealed`, `ICommandHandler/IQueryHandler`, return `ValueTask<T>`, `.ConfigureAwait(false)`
- Validators: `{Command}Validator`, auto-registered
- Constructor injection; thread-safe singletons

## Middleware order

1. ExceptionHandler → ResponseCompression
2. **CORS before HTTPS redirect** (OPTIONS not 307-redirected)
3. HttpsRedirection → SecurityHeaders → Routing
4. `UseAuthentication`
5. `UseModuleMiddlewares` (after auth)
6. RateLimiting → `UseAuthorization` → `MapModules`

## Global state

No mutable static collections under concurrency. Use atomic swaps (`IAuditEnricher[]`) or locks.

## Configuration

- `appsettings.json` in `FSH.Starter.Api/`
- Bind: `AddOptions<T>().BindConfiguration(nameof(T))`
- Section name = type name (e.g., `JwtOptions`, `Storage` not `StorageOptions`)
- Validate with `.ValidateDataAnnotations().ValidateOnStart()`
- Production fail-fast: missing `DatabaseOptions:ConnectionString`, `CachingOptions:Redis`, or `JwtOptions:SigningKey` throws
- Composition: `builder.AddHeroPlatform(o => { o.Enable... })` / `app.UseHeroPlatform(...)`
