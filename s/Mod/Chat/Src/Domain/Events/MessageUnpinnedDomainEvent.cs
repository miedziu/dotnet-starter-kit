using FSH.Framework.Core.Domain;

namespace FSH.Mod.Chat.Domain.Events;

public sealed record MessageUnpinnedDomainEvent(
    Guid ChannelId,
    Guid MessageId,
    string UnpinnedByUserId,
    Guid EventId,
    DateTimeOffset OccurredAt) : DomainEvent(EventId, OccurredAt);