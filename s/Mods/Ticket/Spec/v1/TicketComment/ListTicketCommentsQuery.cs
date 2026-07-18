using Mediator;

namespace FSH.Mods.Ticket.Spec.v1.TicketComment;

public sealed record ListTicketCommentsQuery(Guid TicketId) : IQuery<IReadOnlyList<TicketCommentDto>>;