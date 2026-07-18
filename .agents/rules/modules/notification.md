# Module: Notifications

Per-user in-app inbox (bell icon) driven by cross-module integration events, with live SignalR push. Module `Order = 750` (**before Chat 800**).

**Entities:** `Notification` (aggregate: `UserId`, `Type`, `Title`/`Body`/`Link`/`Source`/`MetadataJson`/`ReadAtUtc`). `NotificationsDbContext`. Consumes integration events (e.g. Chat's `MentionedInChannelIntegrationEvent`).

**Areas:** List, GetUnreadCount, MarkRead, MarkAllRead.

## Gotchas

- **It's a consumer.** New notification types come from **handling another module's integration event** (`AddIntegrationEventHandlers`), not from new endpoints. Handler writes inbox row **and** pushes `"NotificationCreated"` to SignalR group `user:{userId}` via `IHubContext<AppHub>`.
- **Order matters** — Notifications (750) must load before any publisher whose events it consumes (Chat 800).
- In-memory bus runs handlers **synchronously** in publisher's request scope — keep handler minimal; exception surfaces to request. See `eventing.md`.
- Inbox rows **denormalized** (Title/Body/Link/MetadataJson copied in) so rendering never calls back to source module. `MarkRead` is idempotent (`ReadAtUtc ??= now`).