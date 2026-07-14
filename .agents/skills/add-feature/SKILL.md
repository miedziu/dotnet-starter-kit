---
name: add-feature
description: Add a vertical-slice feature (command/query + handler + validator + endpoint) to an existing FSH module. Use when adding an API endpoint or business operation to a module that already exists.
argument-hint: [ModuleName] [Area] [FeatureName]
---

# Add Feature

Vertical slice split across two projects: request/response types in `.Contracts`, handler/validator/endpoint in runtime. See `api-conventions.md`.

## Layout

```
src/Modules/{X}/Modules.{X}.Contracts/v1/{Area}/{Feature}Command.cs
src/Modules/{X}/Modules.{X}.Contracts/Dtos/{Entity}Dto.cs
src/Modules/{X}/Modules.{X}/Features/v1/{Area}/{Feature}/
├── {Feature}CommandHandler.cs
├── {Feature}CommandValidator.cs
└── {Feature}Endpoint.cs
```

## Step 1 — Command/Query (Contracts)

```csharp
namespace FSH.Modules.{X}.Contracts.v1.{Area};

public sealed record Create{Entity}Command(string Name, decimal PriceAmount, string PriceCurrency)
    : ICommand<Guid>;
```

DTOs in `Contracts/Dtos/`. Paginated queries return `PagedResponse<T>`.

## Step 2 — Handler (runtime `Features/`)

Inject `{X}DbContext` directly — **no repository**. `public sealed`, primary ctor, `ValueTask<T>`, `.ConfigureAwait(false)`.

```csharp
public sealed class Create{Entity}CommandHandler(CatalogDbContext dbContext)
    : ICommandHandler<Create{Entity}Command, Guid>
{
    public async ValueTask<Guid> Handle(Create{Entity}Command command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var entity = {Entity}.Create(command.Name, new Money(command.PriceAmount, command.PriceCurrency));
        dbContext.{Entities}.Add(entity);
        await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
        return entity.Id;
    }
}
```

Throw `NotFoundException` / `CustomException(msg, errors, HttpStatusCode)`.

## Step 3 — Validator (same folder, required)

```csharp
public sealed class Create{Entity}CommandValidator : AbstractValidator<Create{Entity}Command>
{
    public Create{Entity}CommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PriceCurrency).NotEmpty().Length(3);
    }
}
```

## Step 4 — Endpoint (same folder)

```csharp
public static class Create{Entity}Endpoint
{
    internal static RouteHandlerBuilder MapCreate{Entity}Endpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/{entities}",
                async (Create{Entity}Command command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command, ct)))
            .WithName("Create{Entity}")
            .WithSummary("Create a {entity}")
            .RequirePermission({X}Permissions.{Entities}.Create)
            .WithIdempotency();
}
```

## Step 5 — Wire in `{X}Module.MapEndpoints`

```csharp
group.MapCreate{Entity}Endpoint();
```

## Step 6 — Verify

```bash
dotnet build src/FSH.Starter.slnx   # 0 warnings
```

## Checklist

- [ ] Command/Query in Contracts (`using Mediator;`)
- [ ] Handler: `public sealed`, injects `{X}DbContext`, `ValueTask<T>` + `.ConfigureAwait(false)`
- [ ] `{Name}Validator` exists
- [ ] Endpoint: `.RequirePermission(...)`, `.WithName/.WithSummary`, `.WithIdempotency()` if POST
- [ ] Wired in module's `MapEndpoints`
- [ ] Build 0 warnings