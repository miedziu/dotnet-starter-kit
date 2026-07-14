using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Tickets.Contracts.Authorization;
using FSH.Modules.Tickets.Contracts.v1.Tickets;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using FSH.Framework.Core.Exceptions;
using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Tickets.Contracts.Dtos;
using FSH.Modules.Tickets.Data;
using FSH.Modules.Tickets.Domain;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Tickets.Features.v1;

public static class ListTicketCommentsEndpoint
{
    internal static RouteHandlerBuilder MapListTicketCommentsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/tickets/{ticketId:guid}/comments",
                async (Guid ticketId, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(new ListTicketCommentsQuery(ticketId), ct)))
            .WithName("ListTicketComments")
            .WithSummary("List the comments on a ticket")
            .RequirePermission(TicketsPermissions.Tickets.View);
    }
}

public sealed class ListTicketCommentsQueryHandler(
    TicketsDbContext dbContext,
    IUserProfileService userProfileService)
    : IQueryHandler<ListTicketCommentsQuery, IReadOnlyList<TicketCommentDto>>
{
    public async ValueTask<IReadOnlyList<TicketCommentDto>> Handle(
        ListTicketCommentsQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        // 404 for a non-existent ticket rather than a misleading empty 200 — this also keeps the
        // endpoint from being used to probe which ticket ids exist.
        var ticketExists = await dbContext.Tickets
            .AsNoTracking()
            .AnyAsync(t => t.Id == query.TicketId, cancellationToken)
            .ConfigureAwait(false);
        if (!ticketExists)
        {
            throw new NotFoundException($"Ticket {query.TicketId} not found.");
        }

        var comments = await dbContext.TicketComments
            .AsNoTracking()
            .Where(c => c.TicketId == query.TicketId)
            .OrderBy(c => c.CreatedAtUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var commentsDTOs = (await Task.WhenAll(comments.Select(c => c.ToDto(userProfileService, cancellationToken).AsTask())).ConfigureAwait(false)).ToList();

        return commentsDTOs;
    }
}