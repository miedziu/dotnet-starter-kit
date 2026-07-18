using Mediator;

namespace FSH.Mods.Ticket.Spec.v1.Ticket;

public sealed record ResolveTicketCommand(Guid TicketId, string? ResolutionNote = null) : ICommand<Guid>;