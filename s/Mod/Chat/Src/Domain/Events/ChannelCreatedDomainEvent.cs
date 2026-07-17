using FSH.Framework.Core.Domain;
using FSH.Modules.Chat.Contracts.v1.Dtos;

namespace FSH.Modules.Chat.Domain.Events;

public sealed record ChannelCreatedDomainEvent(
    Guid ChannelId,
    ChannelType Type,
    string? Name,
    string CreatedByUserId,
    Guid EventId,
    DateTimeOffset OccurredAt) : DomainEvent(EventId, OccurredAt);