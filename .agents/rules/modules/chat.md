# Module: Chat

Slack-style messaging: 1:1 DMs, group DMs, named channels, threads, reactions, mentions, pins. Module `Order = 800` (after Notifications 750).

**Entities:** `ChatChannel` + `ChannelMember`; `Message` + `MessageAttachment`/`MessageMention`/`MessageReaction`. `ChatDbContext : BaseDbContext`, schema `chat`. Publishes `MentionedInChannelIntegrationEvent`.

**Areas:** Channels, Messages, Reactions, Search.

## Gotchas

- **EF value-generation for nav children** — `MessageConfiguration` sets `Property(x => x.Id).ValueGeneratedNever()` for child collections. Domain assigns `Guid.CreateVersion7()` in factories; without this EF treats as `Modified` → UPDATE instead of INSERT.
- `ChatDbContext` calls **`base.OnModelCreating` LAST**.
- **`ChannelAuthorization`**: `RequireMember` throws **NotFound (404)** (not 403); `RequireAdmin` throws `ForbiddenException`. Use in every handler.
- **SignalR via `IHubContext<AppHub>`** (shared hub), groups `channel:{id}`. Hub reads user via `Context.User`, not `ICurrentUser`.
- SendMessage publishes `MentionedInChannelIntegrationEvent` per distinct mentioned user; Notifications consumes it.
- DMs use sorted `DirectKey` (`"{lo}:{hi}"`) for find-or-create; **threads are single-level only**.
- Chat attachments use `ChatChannelFileAccessPolicy` (OwnerType `"ChatChannel"`): attach/read require membership, delete is uploader-only.