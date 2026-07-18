using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Mods.Identity.Spec.Services;
using FSH.Mods.Ticket.Data;
using FSH.Mods.Ticket.Domain;
using FSH.Mods.Ticket.Spec;
using FSH.Mods.Ticket.Spec.v1;
using FSH.Mods.Ticket.Spec.v1.Ticket;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Mods.Ticket.Features.v1;

public static class GetTicketByIdEndpoint
{
    internal static RouteHandlerBuilder MapGetTicketByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/tickets/{ticketId:guid}",
                async (Guid ticketId, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(new GetTicketByIdQuery(ticketId), ct)))
            .WithName("GetTicketById")
            .WithSummary("Get a ticket by id")
            .RequirePermission(TicketPermissions.Ticket.View);
    }
}

public sealed class GetTicketByIdQueryHandler(
    TicketDbContext dbContext,
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