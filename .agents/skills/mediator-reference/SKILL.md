---
name: mediator-reference
description: CQRS interface reference for FSH. This project uses the Mediator source generator, NOT MediatR. Reference when implementing commands, queries, and handlers.
user-invocable: false
---

# Mediator Reference

**FSH uses `Mediator` source-generator (`using Mediator;`), NOT `MediatR`.** Different interfaces — MediatR types won't compile.

## Interfaces

| Purpose | ✅ Mediator | ❌ MediatR |
|---|---|---|
| Command | `ICommand<T>` | `IRequest<T>` |
| Query | `IQuery<T>` | `IRequest<T>` |
| Command handler | `ICommandHandler<T,TResponse>` | `IRequestHandler<…>` |
| Query handler | `IQueryHandler<T,TResponse>` | `IRequestHandler<…>` |
| Notification | `INotification` (`IDomainEvent : INotification`) | `INotification` |

## Pattern

```csharp
// Command/Query → Contracts project
public sealed record Create{Entity}Command(string Name) : ICommand<Guid>;

// Handler → runtime project
public sealed class Create{Entity}CommandHandler({X}DbContext db)
    : ICommandHandler<Create{Entity}Command, Guid>
{
    public async ValueTask<Guid> Handle(Create{Entity}Command command, CancellationToken ct)
    {
        // …
    }
}
```

**Rules:** `ValueTask<T>` (not `Task<T>`); param named `command`/`query`; `public sealed`; `.ConfigureAwait(false)`. Send via `mediator.Send(command, ct)`.

## Registration — the four places

Source generator scans `o.Assemblies` in **two host files**. New module needs **two markers** (Contracts type + module type) in `moduleAssemblies` array — in **both** `Program.cs` and `DbMigrator/Program.cs`:

```csharp
builder.Services.AddMediator(o =>
{
    o.ServiceLifetime = ServiceLifetime.Scoped;
    o.Assemblies = [
        typeof(FSH.Modules.{X}.Contracts.{X}ContractsMarker),
        typeof(FSH.Modules.{X}.{X}Module)];
});
```

See `add-module` for full procedure.

## Common errors

| Symptom | Cause → fix |
|---|---|
| `IRequest<T>` / `IRequestHandler<,>` not found | MediatR interface → use `ICommand`/`IQuery` + `ICommandHandler`/`IQueryHandler` |
| `Task<T>` vs `ValueTask<T>` mismatch | Handler must return `ValueTask<T>` |
| Handler not invoked | Assembly missing from `o.Assemblies` in one or both Program.cs files |