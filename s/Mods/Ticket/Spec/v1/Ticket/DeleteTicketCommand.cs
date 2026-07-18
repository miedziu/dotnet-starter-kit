using Mediator;

namespace FSH.Mods.Ticket.Spec.v1.Ticket;

public sealed record DeleteTicketCommand(Guid TicketId) : ICommand<Unit>;