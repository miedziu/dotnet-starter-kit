using Mediator;

namespace FSH.Mod.Ticket.Spec.v1.Ticket;

public sealed record GetTicketByIdQuery(Guid TicketId) : IQuery<TicketDto>;