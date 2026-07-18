using Mediator;

namespace FSH.Mod.Ticket.Spec.v1.TicketComment;

public sealed record AddTicketCommentCommand(Guid TicketId, string Body) : ICommand<Guid>;