using FSH.Framework.Core.Domain;
using FSH.Mods.Ticket.Spec.v1;

namespace FSH.Mods.Ticket.Domain.Events;

public sealed record TicketCreatedDomainEvent(
    Guid TicketId,
    string Number,
    string Title,
    TicketPriority Priority,
    int? ReporterUserId,
    int? AssignedToUserId,
    Guid EventId,
    DateTimeOffset OccurredAt) : DomainEvent(EventId, OccurredAt);