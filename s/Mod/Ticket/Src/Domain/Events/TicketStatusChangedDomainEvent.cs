using FSH.Framework.Core.Domain;
using FSH.Modules.Tickets.Contracts.v1.Dtos;

namespace FSH.Modules.Tickets.Domain.Events;

public sealed record TicketStatusChangedDomainEvent(
    Guid TicketId,
    TicketStatus PreviousStatus,
    TicketStatus NewStatus,
    Guid EventId,
    DateTimeOffset OccurredAt) : DomainEvent(EventId, OccurredAt);