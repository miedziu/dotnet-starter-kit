---
name: query-patterns
description: Implement read queries — paginated lists, search/filter/sort, and single-entity fetches — the FSH way (DbContext LINQ + PagedResponse). Use when adding GET endpoints. See also add-feature.
---

# Query Patterns

Raw `IQueryable` on DbContext (`AsNoTracking`) with manual pagination. **No generic repository** or `PaginatedListAsync`. Paged results: `PagedResponse<T>`.

## Paginated search query

```csharp
// Contracts/v1/{Area}/
public sealed record Search{Entities}Query(
    string? Search = null,
    bool? IsActive = null,
    int PageNumber = 1,
    int PageSize = 20,
    string? SortBy = null,
    string? SortDir = null) : IQuery<PagedResponse<{Entity}Dto>>;
```

```csharp
// Features/v1/{Area}/Search{Entities}/
public sealed class Search{Entities}QueryHandler({X}DbContext dbContext)
    : IQueryHandler<Search{Entities}Query, PagedResponse<{Entity}Dto>>
{
    public async ValueTask<PagedResponse<{Entity}Dto>> Handle(
        Search{Entities}Query query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        int page = query.PageNumber < 1 ? 1 : query.PageNumber;
        int size = Math.Clamp(query.PageSize, 1, 100);

        var q = dbContext.{Entities}.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Search))
            q = q.Where(x => EF.Functions.ILike(x.Name, $"%{query.Search}%"));
        if (query.IsActive is { } active)
            q = q.Where(x => x.IsActive == active);

        q = ApplySort(q, query.SortBy, query.SortDir);

        long total = await q.LongCountAsync(ct).ConfigureAwait(false);
        var items = await q.Skip((page - 1) * size).Take(size)
            .ToListAsync(ct).ConfigureAwait(false);

        return new PagedResponse<{Entity}Dto>
        {
            Items = items.Select(x => x.ToDto()).ToList(),
            PageNumber = page, PageSize = size,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling(total / (double)size)
        };
    }

    private static IQueryable<{Entity}> ApplySort(IQueryable<{Entity}> q, string? by, string? dir) =>
        by?.ToLowerInvariant() switch
        {
            "name" => string.Equals(dir, "desc", StringComparison.OrdinalIgnoreCase) 
                ? q.OrderByDescending(x => x.Name) : q.OrderBy(x => x.Name),
            _ => q.OrderByDescending(x => x.CreatedOnUtc)
        };
}
```

Soft-delete filters apply automatically. Project to DTO; never return entities.

## Single-entity query

```csharp
public sealed record Get{Entity}Query(Guid Id) : IQuery<{Entity}Dto>;

public sealed class Get{Entity}QueryHandler({X}DbContext dbContext)
    : IQueryHandler<Get{Entity}Query, {Entity}Dto>
{
    public async ValueTask<{Entity}Dto> Handle(Get{Entity}Query query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        var entity = await dbContext.{Entities}.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == query.Id, ct).ConfigureAwait(false)
            ?? throw new NotFoundException($"{Entity} {query.Id} not found");
        return entity.ToDto();
    }
}
```

## Endpoints

```csharp
// list
endpoints.MapGet("/{entities}", async ([AsParameters] Search{Entities}Query query,
        IMediator mediator, CancellationToken ct) => Results.Ok(await mediator.Send(query, ct)))
    .WithName("Search{Entities}").RequirePermission({X}Permissions.{Entities}.View);

// single
endpoints.MapGet("/{entities}/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
    Results.Ok(await mediator.Send(new Get{Entity}Query(id), ct)))
    .WithName("Get{Entity}").RequirePermission({X}Permissions.{Entities}.View);
```

**Paginated query needs validator** (`PageNumber >= 1`, `PageSize` in `[1,100]`).

## When to use Specification

`Specification<T>` for reusable query shapes shared across handlers. Otherwise inline LINQ.