using FSH.Framework.Core.Domain;

namespace FSH.Mod.Ticket.Domain.Events;

public sealed record TicketCreatedDomainEvent(
    Guid TicketId,
    string Number,
    string Title,
    TicketPriority Priority,
    int? ReporterUserId,
    int? AssignedToUserId,
    Guid EventId,
    DateTimeOffset OccurredAt) : DomainEvent(EventId, OccurredAt);