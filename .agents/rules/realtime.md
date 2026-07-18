# Realtime — SignalR & SSE (backend)

`s/Lib/Web/Realtime/` + `Sse/`. For the frontend side see `frontend/shared.md` + `frontend/dashboard.md`.

## SignalR (`AppHub`)

`[Authorize] AppHub` at `/api/v1/realtime/hub`. Groups: `user:{userId}`, `channel:{channelId}`.

- **Channel-group join:** connect-time + on-demand. `OnConnectedAsync` auto-joins `user:{id}` + all `channel:{id}` user is member of. New channels after socket live require `JoinChannel(channelId)` hub method.
- **⚠️ Use `Context.User`, NOT `ICurrentUser`.** `ICurrentUser` returns nulls inside hub.
- Broadcasts scoped to groups (`user:{id}`, `channel:{id}`), never `Clients.All`.
- Redis backplane auto-added when `CachingOptions:Redis` set (prefix `fsh-signalr`).
- Push via `IHubContext<AppHub>` to group `user:{userId}`.
- `IPresenceTracker` (in-memory, per-host). Mod supply `IChannelMembershipChecker`/`IUserChannelLookup` adapters.

## SSE — two-step token

EventSource can't send Authorization → token handshake:

1. `POST /api/v1/sse/token` (authorized) → opaque Guid, single-use, 30s TTL in `IDistributedCache`
2. `GET /api/v1/sse/stream?token={guid}` (anonymous) → `text/event-stream`, `X-Accel-Buffering: no`, 15s heartbeat

`SseConnectionManager` (singleton, `ConcurrentDictionary`, cap 100 `DropOldest`): `TrySend(userId)`, `Broadcast()`, `BroadcastAll()`.
