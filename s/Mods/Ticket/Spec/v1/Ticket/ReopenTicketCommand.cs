using Mediator;

namespace FSH.Mods.Ticket.Spec.v1.Ticket;

public sealed record ReopenTicketCommand(Guid TicketId) : ICommand<Guid>;