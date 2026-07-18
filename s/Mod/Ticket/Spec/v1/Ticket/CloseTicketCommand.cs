using Mediator;

namespace FSH.Mod.Ticket.Spec.v1.Ticket;

public sealed record CloseTicketCommand(Guid TicketId) : ICommand<Guid>;