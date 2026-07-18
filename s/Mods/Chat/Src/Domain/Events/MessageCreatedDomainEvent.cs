using FSH.Framework.Core.Domain;

namespace FSH.Mods.Chat.Domain.Events;

public sealed record MessageCreatedDomainEvent(
    Guid ChannelId,
    Guid MessageId,
    string AuthorUserId,
    Guid? ParentMessageId,
    Guid EventId,
    DateTimeOffset OccurredAt) : DomainEvent(EventId, OccurredAt);