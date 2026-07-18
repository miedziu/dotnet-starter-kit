using Mediator;

namespace FSH.Mods.Ticket.Spec.v1.Ticket;

public sealed record RestoreTicketCommand(Guid TicketId) : ICommand<Guid>;