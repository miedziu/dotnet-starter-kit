using FSH.Framework.Core.Domain;
using FSH.Modules.Tickets.Contracts.Dtos;

namespace FSH.Modules.Tickets.Domain.Events;

public sealed record TicketCreatedDomainEvent(
    Guid TicketId,
    string Number,
    string Title,
    TicketPriority Priority,
    int? ReporterUserId,
    int? AssignedToUserId,
    Guid EventId,
    DateTimeOffset OccurredAt) : DomainEvent(EventId, OccurredAt);