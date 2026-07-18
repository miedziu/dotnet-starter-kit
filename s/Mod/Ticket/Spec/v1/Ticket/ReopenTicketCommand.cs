using Mediator;

namespace FSH.Mod.Ticket.Spec.v1.Ticket;

public sealed record ReopenTicketCommand(Guid TicketId) : ICommand<Guid>;