namespace FSH.Mod.Ticket.Spec.v1;

public sealed record TicketCommentDto(
    Guid Id,
    Guid TicketId,
    Guid AuthorUserId,
    string Body,
    DateTime CreatedAtUtc);