using Mediator;

namespace FSH.Mod.Ticket.Spec.v1.Ticket;

public sealed record UpdateTicketCommand(
    Guid TicketId,
    string Title,
    string? Description = null,
    TicketPriority Priority = TicketPriority.Medium) : ICommand<Guid>;