using FSH.Framework.Core.Domain;

namespace FSH.Modules.Tickets.Domain.Events;

public sealed record TicketAssignedDomainEvent(
    Guid TicketId,
    int? PreviousAssigneeUserId,
    int? NewAssigneeUserId,
    Guid EventId,
    DateTimeOffset OccurredAt) : DomainEvent(EventId, OccurredAt);
