using FSH.Framework.Shared.Persistence;
using Mediator;

namespace FSH.Mod.Ticket.Spec.v1.Ticket;

public sealed record SearchTicketsQuery : IQuery<PagedResponse<TicketDto>>
{
    public string? Search { get; init; }
    public TicketStatus? Status { get; init; }
    public TicketPriority? Priority { get; init; }
    public Guid? AssignedToUserId { get; init; }
    public Guid? ReporterUserId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SortBy { get; init; }
    public string? SortDir { get; init; }
}