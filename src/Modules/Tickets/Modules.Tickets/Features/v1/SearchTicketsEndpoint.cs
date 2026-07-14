using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Tickets.Contracts.Authorization;
using FSH.Modules.Tickets.Contracts.Dtos;
using FSH.Modules.Tickets.Contracts.v1.Tickets;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using FSH.Modules.Tickets.Domain;
using Microsoft.EntityFrameworkCore;
using FSH.Framework.Shared.Persistence;
using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Tickets.Data;

namespace FSH.Modules.Tickets.Features.v1;

public static class SearchTicketsEndpoint
{
    internal static RouteHandlerBuilder MapSearchTicketsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/tickets",
                async (
                    string? search,
                    TicketStatus? status,
                    TicketPriority? priority,
                    Guid? assignedToUserId,
                    Guid? reporterUserId,
                    int? pageNumber,
                    int? pageSize,
                    string? sortBy,
                    string? sortDir,
                    IMediator mediator,
                    CancellationToken ct) =>
                {
                    var query = new SearchTicketsQuery
                    {
                        Search = search,
                        Status = status,
                        Priority = priority,
                        AssignedToUserId = assignedToUserId,
                        ReporterUserId = reporterUserId,
                        PageNumber = pageNumber ?? 1,
                        PageSize = pageSize ?? 20,
                        SortBy = sortBy,
                        SortDir = sortDir,
                    };
                    return Results.Ok(await mediator.Send(query, ct));
                })
            .WithName("SearchTickets")
            .WithSummary("Search tickets")
            .RequirePermission(TicketsPermissions.Tickets.View);
    }
}

public sealed class SearchTicketsQueryHandler(
    TicketsDbContext dbContext,
    IUserProfileService userProfileService)
    : IQueryHandler<SearchTicketsQuery, PagedResponse<TicketDto>>
{
    public async ValueTask<PagedResponse<TicketDto>> Handle(SearchTicketsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        int page = query.PageNumber < 1 ? 1 : query.PageNumber;
        int size = query.PageSize is < 1 or > 200 ? 20 : query.PageSize;

        var q = dbContext.Tickets.AsNoTracking().AsQueryable();

        if (query.Status is { } status)
        {
            q = q.Where(t => t.Status == status);
        }
        if (query.Priority is { } priority)
        {
            q = q.Where(t => t.Priority == priority);
        }
        if (query.AssignedToUserId is { } assignee)
        {
            var assigneeIntId = await userProfileService.GetIntIdAsync(assignee.ToString(), cancellationToken);
            q = q.Where(t => t.AssignedToUserId == assigneeIntId);
        }
        if (query.ReporterUserId is { } reporter)
        {
            var reporterIntId = await userProfileService.GetIntIdAsync(reporter.ToString(), cancellationToken);
            q = q.Where(t => t.ReporterUserId == reporterIntId);

        }
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string term = query.Search.Trim();
            q = q.Where(t =>
                EF.Functions.ILike(t.Title, $"%{term}%") ||
                EF.Functions.ILike(t.Number, $"%{term}%") ||
                (t.Description != null && EF.Functions.ILike(t.Description, $"%{term}%")));
        }

        q = ApplySort(q, query.SortBy, query.SortDir);

        long total = await q.LongCountAsync(cancellationToken).ConfigureAwait(false);

        // Project with comment count via subquery so we don't have to
        // materialize the comments collection just to count it.
        var projected = await q
            .Skip((page - 1) * size)
            .Take(size)
            .Select(t => new
            {
                Ticket = t,
                CommentCount = dbContext.TicketComments.Count(c => c.TicketId == t.Id),
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var items = (await Task.WhenAll(projected.Select(p => p.Ticket.ToDto(userProfileService, cancellationToken).AsTask())).ConfigureAwait(false)).ToList();

        return new PagedResponse<TicketDto>
        {
            Items = items,
            PageNumber = page,
            PageSize = size,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling(total / (double)size)
        };
    }

    private static IQueryable<Ticket> ApplySort(IQueryable<Ticket> q, string? sortBy, string? sortDir)
    {
        bool desc = !string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase);
        return (sortBy?.ToUpperInvariant()) switch
        {
            "TITLE" => desc ? q.OrderByDescending(t => t.Title) : q.OrderBy(t => t.Title),
            "PRIORITY" => desc ? q.OrderByDescending(t => t.Priority) : q.OrderBy(t => t.Priority),
            "STATUS" => desc ? q.OrderByDescending(t => t.Status) : q.OrderBy(t => t.Status),
            "NUMBER" => desc ? q.OrderByDescending(t => t.Number) : q.OrderBy(t => t.Number),
            _ => desc ? q.OrderByDescending(t => t.CreatedAtUtc) : q.OrderBy(t => t.CreatedAtUtc),
        };
    }
}