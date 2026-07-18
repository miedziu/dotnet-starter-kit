using Mediator;

namespace FSH.Mod.Ticket.Spec.v1.Ticket;

public sealed record CreateTicketCommand(
    string Title,
    string? Description = null,
    TicketPriority Priority = TicketPriority.Medium,
    Guid? AssignedToUserId = null) : ICommand<Guid>;