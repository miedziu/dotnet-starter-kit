using FSH.Framework.Core.Domain;

namespace FSH.Mods.Chat.Domain.Events;

public sealed record MessageDeletedDomainEvent(
    Guid ChannelId,
    Guid MessageId,
    string AuthorUserId,
    Guid EventId,
    DateTimeOffset OccurredAt) : DomainEvent(EventId, OccurredAt);