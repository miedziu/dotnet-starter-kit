using FSH.Framework.Core.Domain;

namespace FSH.Mod.File.Domain.Events;

public sealed record FileFinalizedDomainEvent(
    Guid FileAssetId,
    string OwnerType,
    Guid? OwnerId,
    FileAssetStatus FinalStatus,
    Guid EventId,
    DateTimeOffset OccurredAt) : DomainEvent(EventId, OccurredAt);