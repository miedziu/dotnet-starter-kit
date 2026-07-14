# API conventions

## Endpoints

Static extension on `IEndpointRouteBuilder` → `RouteHandlerBuilder`. Handler delegates to Mediator.

```csharp
public static class RegisterUserEndpoint
{
    internal static RouteHandlerBuilder MapRegisterUserEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/register", (RegisterUserCommand command, IMediator mediator, CancellationToken ct) =>
                mediator.Send(command, ct))
            .WithName("RegisterUser")
            .WithSummary("Register user")
            .RequirePermission(IdentityPermissionConstants.Users.Create);
}
```

- Forward `CancellationToken` to `mediator.Send`
- Wire in module's `MapEndpoints()`; endpoints under `api/v{version:apiVersion}/{module}`
- Use `TypedResults` / `.Produces<T>(...)` for OpenAPI. Add `.WithIdempotency()` on replay-safe POSTs

## CQRS

| Type | Location | Interface |
|---|---|---|
| Command/Query | Modules.{Name}.Contracts | `ICommand<T>` / `IQuery<T>` |
| Handler | Modules.{Name}/Features/ | `ICommandHandler<T,TResponse>` / `IQueryHandler<T,TResponse>` |

- Handlers: `public sealed`, `ValueTask<T>`, `.ConfigureAwait(false)`
- Paginated queries: `IPagedQuery` + `PagedResponse<T>`

## Validation

- `{Command}Validator` in same file as handler and endpoint (Single File Slice pattern)
- Every command + paginated query needs validator
- Validators run via `ValidationBehavior<,>` before handler

## Exceptions → ProblemDetails

| Throw | HTTP |
|---|---|
| `NotFoundException` | 404 |
| `ForbiddenException` | 403 |
| `UnauthorizedException` | 401 |
| `CustomException(msg, errors?, HttpStatusCode)` | as specified (default 400) |

Background loops: `catch (Exception)` with context, exclude `OperationCanceledException`.

## Permissions

- Constants in `Shared/Identity/*Permissions.cs`
- Apply with `.RequirePermission(...)`
- **Never duplicate `IRequiredPermissionMetadata`** — disables all permission gates

## Specifications

Use `Specification<T>` for composable queries. `AsNoTracking = true` by default.

## Adding a feature

1. Command/Query in `Modules.{Name}.Contracts/v1/{Area}/{Feature}.cs`
2. In order: Endpoint + Validator + Handler in `Modules.{Name}/Features/v1/{Area}/{Feature}.cs` (Single File Slice)
3. Wire endpoint in module's `MapEndpoints()`
