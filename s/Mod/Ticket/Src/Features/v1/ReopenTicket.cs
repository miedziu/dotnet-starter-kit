using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Modules.Tickets.Contracts.Authorization;
using FSH.Modules.Tickets.Contracts.v1.Tickets;
using FSH.Modules.Tickets.Data;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Tickets.Features.v1;

public static class ReopenTicketEndpoint
{
    internal static RouteHandlerBuilder MapReopenTicketEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/tickets/{ticketId:guid}/reopen",
                async (Guid ticketId, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(new ReopenTicketCommand(ticketId), ct)))
            .WithName("ReopenTicket")
            .WithSummary("Reopen a resolved or closed ticket")
            .RequirePermission(TicketsPermissions.Tickets.Reopen)
            .WithIdempotency();
    }
}

public sealed class ReopenTicketCommandHandler(TicketsDbContext dbContext)
    : ICommandHandler<ReopenTicketCommand, Guid>
{
    public async ValueTask<Guid> Handle(ReopenTicketCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var ticket = await dbContext.Tickets
            .FirstOrDefaultAsync(t => t.Id == command.TicketId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Ticket {command.TicketId} not found.");

        ticket.Reopen();
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return ticket.Id;
    }
}