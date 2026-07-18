using FSH.Framework.Core.Domain;

namespace FSH.Mod.Ticket.Domain.Events;

public sealed record TicketStatusChangedDomainEvent(
    Guid TicketId,
    TicketStatus PreviousStatus,
    TicketStatus NewStatus,
    Guid EventId,
    DateTimeOffset OccurredAt) : DomainEvent(EventId, OccurredAt);