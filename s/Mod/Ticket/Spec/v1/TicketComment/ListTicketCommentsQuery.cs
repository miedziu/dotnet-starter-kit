using FSH.Modules.Tickets.Contracts.v1.Dtos;
using Mediator;

namespace FSH.Modules.Tickets.Contracts.v1.Tickets;

public sealed record ListTicketCommentsQuery(Guid TicketId) : IQuery<IReadOnlyList<TicketCommentDto>>;