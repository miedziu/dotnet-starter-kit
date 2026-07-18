using FSH.Framework.Core.Domain;

namespace FSH.Mod.Chat.Domain.Events;

public sealed record ChannelCreatedDomainEvent(
    Guid ChannelId,
    ChannelType Type,
    string? Name,
    string CreatedByUserId,
    Guid EventId,
    DateTimeOffset OccurredAt) : DomainEvent(EventId, OccurredAt);