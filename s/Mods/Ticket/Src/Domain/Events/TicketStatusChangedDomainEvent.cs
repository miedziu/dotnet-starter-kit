using FSH.Framework.Core.Domain;
using FSH.Mods.Ticket.Spec.v1;

namespace FSH.Mods.Ticket.Domain.Events;

public sealed record TicketStatusChangedDomainEvent(
    Guid TicketId,
    TicketStatus PreviousStatus,
    TicketStatus NewStatus,
    Guid EventId,
    DateTimeOffset OccurredAt) : DomainEvent(EventId, OccurredAt);