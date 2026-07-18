using Mediator;

namespace FSH.Mod.Ticket.Spec.v1.Ticket;

public sealed record DeleteTicketCommand(Guid TicketId) : ICommand<Unit>;