using Mediator;

namespace FSH.Mod.Ticket.Spec.v1.Ticket;

public sealed record RestoreTicketCommand(Guid TicketId) : ICommand<Guid>;