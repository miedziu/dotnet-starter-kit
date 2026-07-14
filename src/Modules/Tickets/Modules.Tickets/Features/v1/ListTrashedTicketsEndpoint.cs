using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Tickets.Contracts.Authorization;
using FSH.Modules.Tickets.Contracts.v1.Tickets;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using FSH.Framework.Persistence;
using FSH.Framework.Shared.Persistence;
using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Tickets.Contracts.Dtos;
using FSH.Modules.Tickets.Data;
using FSH.Modules.Tickets.Domain;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Tickets.Features.v1;

public static class ListTrashedTicketsEndpoint
{
    internal static RouteHandlerBuilder MapListTrashedTicketsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/tickets/trash",
                async (int? pageNumber, int? pageSize, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(
                        new ListTrashedTicketsQuery(pageNumber ?? 1, pageSize ?? 20), ct)))
            .WithName("ListTrashedTickets")
            .WithSummary("List soft-deleted tickets")
            .RequirePermission(TicketsPermissions.Tickets.Restore);
    }
}

public sealed class ListTrashedTicketsQueryHandler(
    TicketsDbContext dbContext,
    IUserProfileService userProfileService)
    : IQueryHandler<ListTrashedTicketsQuery, PagedResponse<TicketDto>>
{
    public async ValueTask<PagedResponse<TicketDto>> Handle(
        ListTrashedTicketsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        int page = query.PageNumber < 1 ? 1 : query.PageNumber;
        int size = query.PageSize is < 1 or > 200 ? 20 : query.PageSize;

        var q = dbContext.Tickets
            .AsNoTracking()
            .IgnoreQueryFilters([QueryFilters.SoftDelete])
            .Where(t => t.DeletedAt != null)
            .OrderByDescending(t => t.DeletedAt);

        long total = await q.LongCountAsync(cancellationToken).ConfigureAwait(false);
        var tickets = await q
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var items = (await Task.WhenAll(tickets.Select(t => t.ToDto(0, userProfileService, cancellationToken).AsTask())).ConfigureAwait(false)).ToList();
        return new PagedResponse<TicketDto>
        {
            Items = items,
            PageNumber = page,
            PageSize = size,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling(total / (double)size)
        };
    }
}