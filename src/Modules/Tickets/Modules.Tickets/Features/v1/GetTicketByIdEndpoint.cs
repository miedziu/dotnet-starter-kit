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

public static class GetTicketByIdEndpoint
{
    internal static RouteHandlerBuilder MapGetTicketByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/tickets/{ticketId:guid}",
                async (Guid ticketId, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(new GetTicketByIdQuery(ticketId), ct)))
            .WithName("GetTicketById")
            .WithSummary("Get a ticket by id")
            .RequirePermission(TicketsPermissions.Tickets.View);
    }
}

public sealed class GetTicketByIdQueryHandler(
    TicketsDbContext dbContext,
    IUserProfileService userProfileService)
    : IQueryHandler<GetTicketByIdQuery, TicketDto>
{
    public async ValueTask<TicketDto> Handle(GetTicketByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var ticket = await dbContext.Tickets
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == query.TicketId, cancellationToken)
            .ConfigureAwait(false);

        if (ticket is null)
        {
            throw new NotFoundException($"Ticket {query.TicketId} not found.");
        }

        int commentCount = await dbContext.TicketComments
            .CountAsync(c => c.TicketId == ticket.Id, cancellationToken)
            .ConfigureAwait(false);

        return await ticket.ToDto(commentCount, userProfileService, cancellationToken).ConfigureAwait(false);
    }
}