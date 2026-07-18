namespace FSH.Mods.Ticket.Spec.v1;

/// <summary>
/// Read-side projection of a ticket. Includes resolution metadata
/// (resolvedAt, resolutionNote) when the ticket has been resolved at
/// least once. Comments are returned via the dedicated
/// `/tickets/{id}/comments` endpoint to keep this DTO small.
/// </summary>
public sealed record TicketDto(
    Guid Id,
    string Number,
    string Title,
    string? Description,
    TicketStatus Status,
    TicketPriority Priority,
    Guid ReporterUserId,
    Guid? AssignedToUserId,
    string? ResolutionNote,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    DateTime? ResolvedAtUtc,
    DateTime? ClosedAtUtc,
    int CommentCount,
    DateTimeOffset? DeletedAt = null,
    string? DeletedBy = null);