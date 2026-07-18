using FSH.Framework.Core.Domain;

namespace FSH.Mod.Chat.Domain.Events;

public sealed record ChannelMemberRemovedDomainEvent(
    Guid ChannelId,
    string RemovedUserId,
    string RemovedByUserId,
    Guid EventId,
    DateTimeOffset OccurredAt) : DomainEvent(EventId, OccurredAt);