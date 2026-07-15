namespace FSH.Modules.Tickets.Contracts.v1.Dtos;

public sealed record TicketCommentDto(
    Guid Id,
    Guid TicketId,
    Guid AuthorUserId,
    string Body,
    DateTime CreatedAtUtc);