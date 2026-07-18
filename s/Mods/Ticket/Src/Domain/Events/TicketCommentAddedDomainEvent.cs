using FSH.Framework.Core.Domain;

namespace FSH.Mods.Ticket.Domain.Events;

public sealed record TicketCommentAddedDomainEvent(
    Guid TicketId,
    Guid CommentId,
    int? AuthorUserId,
    Guid EventId,
    DateTimeOffset OccurredAt) : DomainEvent(EventId, OccurredAt);