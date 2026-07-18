using FSH.Framework.Core.Domain;

namespace FSH.Mod.File.Domain.Events;

public sealed record FileSoftDeletedDomainEvent(
    Guid FileAssetId,
    string ActorUserId,
    Guid EventId,
    DateTimeOffset OccurredAt) : DomainEvent(EventId, OccurredAt);