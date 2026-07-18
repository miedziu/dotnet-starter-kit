using FSH.Framework.Core.Domain;

namespace FSH.Mod.Chat.Domain.Events;

public sealed record MessagePinnedDomainEvent(
    Guid ChannelId,
    Guid MessageId,
    string PinnedByUserId,
    Guid EventId,
    DateTimeOffset OccurredAt) : DomainEvent(EventId, OccurredAt);