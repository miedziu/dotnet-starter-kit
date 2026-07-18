using Mediator;

namespace FSH.Mods.Ticket.Spec.v1.Ticket;

public sealed record CloseTicketCommand(Guid TicketId) : ICommand<Guid>;