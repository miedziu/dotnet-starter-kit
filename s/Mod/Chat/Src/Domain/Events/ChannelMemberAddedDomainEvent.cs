using FSH.Framework.Core.Domain;

namespace FSH.Mod.Chat.Domain.Events;

public sealed record ChannelMemberAddedDomainEvent(
    Guid ChannelId,
    string AddedUserId,
    string AddedByUserId,
    Guid EventId,
    DateTimeOffset OccurredAt) : DomainEvent(EventId, OccurredAt);