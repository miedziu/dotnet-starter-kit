using Mediator;

namespace FSH.Mod.Ticket.Spec.v1.TicketComment;

public sealed record ListTicketCommentsQuery(Guid TicketId) : IQuery<IReadOnlyList<TicketCommentDto>>;