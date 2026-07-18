# Caching

`s/Lib/Caching/`. Use `HybridCache` (L1 in-memory + optional L2 Redis), not `IDistributedCache`.

## What's registered

`AddHeroCaching(config)` registers `HybridCache`. Inject `HybridCache`.

- `CachingOptions.Redis` empty → in-memory only (dev). Set → shared `ConnectionMultiplexer` for L2 + DataProtection key ring.
- Defaults: 1h total expiration, 2min L1 expiration.
- `ObservableHybridCache` decorates `HybridCache` for OpenTelemetry — inject `HybridCache` directly.

## Pattern

```csharp
var perms = await cache.GetOrCreateAsync(
    CacheKeys.UserPermissions(userId),
    async ct => await LoadPermissionsAsync(userId, ct),
    tags: [CacheKeys.Tags.Permissions, CacheKeys.Tags.User(userId)],
    cancellationToken: ct);
```

- **Keys/tags in `CacheKeys.cs`** — don't inline strings. Existing: `UserPermissions(userId)`, `Theme()`, `IdempotencyEntry(key)`, `ImpersonationGrantStatus(jti)`; tags: `Permissions`, `Themes`, `Idempotency`, `User(id)`.
- Invalidate with `RemoveAsync(key)` or `RemoveByTagAsync(tag)` in mutation handler.
- `GetOrCreateAsync` provides stampede protection.

## Gotchas

- **No L1 backplane.** `RemoveByTagAsync` on one node doesn't evict L1 on peers — cross-node staleness bounded by 2-min local expiration. Keep local expiration short for hot, mutable data.
- Use `IDistributedCache` only where framework does (idempotency probe-read) — prefer `HybridCache`.
